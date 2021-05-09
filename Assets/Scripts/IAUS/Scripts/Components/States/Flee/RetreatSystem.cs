using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS2.Component;
using Unity.Entities;
using Unity.Burst;
using Components.MovementSystem;


[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<FleeActionTag, Retreat, IAUS.ECS2.Systems.Reactive.RetreatTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<FleeActionTag, Retreat, IAUS.ECS2.Systems.Reactive.RetreatTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<FleeActionTag, Retreat, IAUS.ECS2.Systems.Reactive.RetreatTagReactor>.ManageComponentRemovalJob))]


namespace IAUS.ECS2.Systems.Reactive
{
    public struct RetreatTagReactor : IComponentReactorTagsForAIStates<FleeActionTag, Retreat>
    {
        public void ComponentAdded(Entity entity, ref FleeActionTag newComponent, ref Retreat AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref Retreat AIStateCompoment, in FleeActionTag oldComponent)
        {
            if (AIStateCompoment.Escape)
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
            }
            else
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime * 2;
            }
        }

        public void ComponentValueChanged(Entity entity, ref FleeActionTag newComponent, ref Retreat AIStateCompoment, in FleeActionTag oldComponent)
        {
        
        }

        public class RetreatReactiveSystem : AIReactiveSystemBase<FleeActionTag, Retreat, RetreatTagReactor>
        {
            protected override RetreatTagReactor CreateComponentReactor()
            {
                return new RetreatTagReactor();
            }
        }
    }

    public class RetreatSystem : SystemBase
    {
        private EntityQuery _componentAddedQuery;
        private EntityQuery _componentRemovedQuery;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Retreat)),  ComponentType.ReadWrite(typeof(Movement))
                , ComponentType.ReadOnly(typeof(LocalToWorld))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<FleeActionTag, Retreat, RetreatTagReactor>.StateComponent)) }
            });
            _componentRemovedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Retreat)), ComponentType.ReadWrite(typeof(Wait)), ComponentType.ReadWrite(typeof(Movement))
                , ComponentType.ReadOnly(typeof(AIReactiveSystemBase<FleeActionTag, Retreat, RetreatTagReactor>.StateComponent)),ComponentType.ReadOnly(typeof(LocalToWorld)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(FleeActionTag)) }
            });

            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new FindRetreatPoint() {
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                transformChunk= GetComponentTypeHandle<LocalToWorld>(true),
                RetreatStateChunk = GetComponentTypeHandle<Retreat>(false)
            }.ScheduleParallel(_componentAddedQuery, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new ArriveAtRetreatPoint()
            {
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                RetreatChunk = GetComponentTypeHandle<Retreat>(true),
                WaitChunk = GetComponentTypeHandle<Wait>(false)
            }.ScheduleParallel(_componentRemovedQuery, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;

        }

        //Needs To be rewritten with be logic but is functional right now 
        [BurstCompile]
        public struct FindRetreatPoint : IJobChunk
        {
            [ReadOnly]public ComponentTypeHandle<LocalToWorld> transformChunk;
            public ComponentTypeHandle<Retreat> RetreatStateChunk;
            public ComponentTypeHandle<Movement> MovementChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<LocalToWorld> transforms = chunk.GetNativeArray(transformChunk);
                NativeArray<Retreat> States = chunk.GetNativeArray(RetreatStateChunk);
                NativeArray<Movement> movements = chunk.GetNativeArray(MovementChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Movement move = movements[i];
                    Retreat retreat = States[i];
                    Utilities.GlobalFunctions.RandomPoint(transforms[i].Position, States[i].EscapeRange, out Vector3 temp);
                    retreat.StartingDistance = Vector3.Distance(transforms[i].Position, temp);
                   retreat.EscapePoint= move.TargetLocation = temp;
                    move.CanMove = true;
                    move.SetTargetLocation = true;

                    movements[i] = move;
                    States[i] = retreat;
                }
            }
        }
        [BurstCompile]
        public struct ArriveAtRetreatPoint : IJobChunk
        {
            public ComponentTypeHandle<Wait> WaitChunk;
            public ComponentTypeHandle<Retreat> RetreatChunk;
            public ComponentTypeHandle<Movement> MovementChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Wait> Waits = chunk.GetNativeArray(WaitChunk);
                NativeArray<Retreat> Retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<Movement> Moves = chunk.GetNativeArray(MovementChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Wait wait = Waits[i];
                    Movement move = Moves[i];
                    Retreat retreat = Retreats[i];
                    wait.Timer = Retreats[i].HideTime;
                    move.CanMove = false;
                    retreat.EscapePoint = new Unity.Mathematics.float3();

                    Retreats[i] = retreat;
                    Waits[i] = wait;
                    Moves[i] = move;
                }
            }
        }
    }
}