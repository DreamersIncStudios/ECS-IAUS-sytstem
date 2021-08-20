using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using IAUS.ECS2.Component;
using Unity.Entities;
using Unity.Burst;
using UnityEngine.AI;
using Components.MovementSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, IAUS.ECS2.Systems.Reactive.RetreatCitizenTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, IAUS.ECS2.Systems.Reactive.RetreatCitizenTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, IAUS.ECS2.Systems.Reactive.RetreatCitizenTagReactor>.ManageComponentRemovalJob))]


namespace IAUS.ECS2.Systems.Reactive
{
    public struct RetreatCitizenTagReactor : IComponentReactorTagsForAIStates<RetreatActionTag, RetreatCitizen>
    {
        public void ComponentAdded(Entity entity, ref RetreatActionTag newComponent, ref RetreatCitizen AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref RetreatCitizen AIStateCompoment, in RetreatActionTag oldComponent)
        {

            if (AIStateCompoment.Escaped|| AIStateCompoment.Status == ActionStatus.Success)
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
                AIStateCompoment.EscapePoint = float3.zero;
            }
            else
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime * 2;
            }

        }

        public void ComponentValueChanged(Entity entity, ref RetreatActionTag newComponent, ref RetreatCitizen AIStateCompoment, in RetreatActionTag oldComponent)
        {
        }

        public class RetreatReactiveSystem : AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, RetreatCitizenTagReactor>
        {
            protected override RetreatCitizenTagReactor CreateComponentReactor()
            {
                return new RetreatCitizenTagReactor();
            }
        }
    }

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]

    public class RetreatMovement : SystemBase
    {
        private EntityQuery _componentAddedQuery;
        private EntityQuery _componentAddedQueryWithWait;

        private EntityQuery _componentRemovedQuery;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        float interval = 4
            ;
        bool runUpdate => interval <= 0.0f;
        protected override void OnCreate()
        {
            base.OnCreate();
            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)), ComponentType.ReadWrite(typeof(RetreatActionTag)), ComponentType.ReadWrite(typeof(Movement)),
                 ComponentType.ReadOnly(typeof(LocalToWorld))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, RetreatCitizenTagReactor>.StateComponent)) }
            });
            _componentAddedQueryWithWait= GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)), ComponentType.ReadWrite(typeof(RetreatActionTag)), ComponentType.ReadWrite(typeof(Wait)),
                 ComponentType.ReadOnly(typeof(LocalToWorld))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, RetreatCitizenTagReactor>.StateComponent)) }
            });
            _componentRemovedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)),  ComponentType.ReadWrite(typeof(Movement))
                , ComponentType.ReadOnly(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, RetreatCitizenTagReactor>.StateComponent)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(RetreatActionTag)) }

            });


            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            if (runUpdate)
            {
                systemDeps = new RetreatMovementUpdate()
                {
                    RetreatChunk = GetComponentTypeHandle<RetreatCitizen>(true),
                    MovementChunk = GetComponentTypeHandle<Movement>(false)
                }.ScheduleParallel(_componentAddedQuery, systemDeps);


                _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
                interval = 3.0f;
            }
            else
            { 
                interval -= 1 / 60.0f;
            }
            systemDeps = new OnCompletionUpdateJob()
            {
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                WaitChunk = GetComponentTypeHandle<Wait>(false)
            }.ScheduleParallel(_componentRemovedQuery, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;

        }


        [BurstCompile]
        public struct RetreatMovementUpdate : IJobChunk
        {
            public ComponentTypeHandle<Movement> MovementChunk;
            [ReadOnly]public ComponentTypeHandle<RetreatCitizen> RetreatChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> toWorldChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Movement> movements = chunk.GetNativeArray(MovementChunk);
                NativeArray<RetreatCitizen> retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<LocalToWorld> transforms = chunk.GetNativeArray(toWorldChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Movement move = movements[i];
                    start:
                    if (NavMesh.SamplePosition(retreats[i].LocationOfLowestThreat, out NavMeshHit hit, 6.0f, NavMesh.AllAreas))
                    {

                        move.TargetLocation = hit.position;
                        move.CanMove = true;
                        move.SetTargetLocation = true;
                    }
                    else
                    {
                        goto start;
                    }
                    movements[i] = move;

                }

            }
        }

        public struct OnCompletionUpdateJob : IJobChunk
        {
            public ComponentTypeHandle<Wait> WaitChunk;
            public ComponentTypeHandle<Movement> MovementChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Wait> Waits = chunk.GetNativeArray(WaitChunk);
                NativeArray<Movement> Moves = chunk.GetNativeArray(MovementChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Wait wait = Waits[i];
                    Movement move = Moves[i];
                        wait.Timer = 300;
                    move.CanMove = false;

                    Waits[i] = wait;
                    Moves[i] = move;
                }
            }
        }

    }
    
}