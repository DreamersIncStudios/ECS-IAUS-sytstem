using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using Components.MovementSystem;
using UnityEngine.AI;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<TraverseActionTag, Traverse, IAUS.ECS.Systems.Reactive.TraverseTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<TraverseActionTag, Traverse, IAUS.ECS.Systems.Reactive.TraverseTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<TraverseActionTag, Traverse, IAUS.ECS.Systems.Reactive.TraverseTagReactor>.ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive
{
    public struct TraverseTagReactor : IComponentReactorTagsForAIStates<TraverseActionTag, Traverse>
    {
        public void ComponentAdded(Entity entity, ref TraverseActionTag newComponent, ref Traverse AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref Traverse AIStateCompoment, in TraverseActionTag oldComponent)
        {
            if (AIStateCompoment.Complete || AIStateCompoment.Status == ActionStatus.Success)
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

        public void ComponentValueChanged(Entity entity, ref TraverseActionTag newComponent, ref Traverse AIStateCompoment, in TraverseActionTag oldComponent)
        {
        }

        public class TraverseReactiveSystem : AIReactiveSystemBase<TraverseActionTag, Traverse, TraverseTagReactor>
        {
            protected override TraverseTagReactor CreateComponentReactor()
            {
                return new TraverseTagReactor();
            }
        }
    }
    public partial class SetSpeedTraverse : ComponentSystem
    {

        private EntityQuery _componentAddedQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Traverse)), ComponentType.ReadWrite(typeof(TraverseActionTag)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer))
                , ComponentType.ReadOnly(typeof(LocalToWorld))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<TraverseActionTag, Traverse, TraverseTagReactor>.StateComponent)) }
            });
      
        }
        protected override void OnUpdate()
        {
            Entities.With(_componentAddedQuery).ForEach((NavMeshAgent agent) => {
                agent.speed = 2.5f;

            });
        }
    }
    public partial class TraverseMovement : SystemBase
    {
        private EntityQuery _componentAddedQuery;
        private EntityQuery _componentRemovedQuery;
        private EntityQuery _traversing;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Traverse)), ComponentType.ReadWrite(typeof(TraverseActionTag)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer))
                , ComponentType.ReadOnly(typeof(LocalToWorld))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<TraverseActionTag, Traverse, TraverseTagReactor>.StateComponent)) }
            });
            _traversing = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Traverse)), ComponentType.ReadWrite(typeof(TraverseActionTag)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer))
                , ComponentType.ReadOnly(typeof(LocalToWorld)),
                    ComponentType.ReadOnly(typeof(AIReactiveSystemBase<TraverseActionTag, Traverse, TraverseTagReactor>.StateComponent)) }
            });
            _componentRemovedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Traverse)), ComponentType.ReadWrite(typeof(Wait)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer)),  ComponentType.ReadWrite(typeof(Movement))
                , ComponentType.ReadOnly(typeof(AIReactiveSystemBase<TraverseActionTag, Traverse, TraverseTagReactor>.StateComponent)),ComponentType.ReadOnly(typeof(LocalToWorld)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(TraverseActionTag)) }
            });

            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new PatrolMovementUpdateJob()
            {
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                TraverseChunk = GetComponentTypeHandle<Traverse>(false),
                ToWorldChunk = GetComponentTypeHandle<LocalToWorld>(true)
            }.ScheduleParallel(_componentAddedQuery, systemDeps);

            systemDeps = new CheckSkipPoint()
            {
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                TraverseChunk = GetComponentTypeHandle<Traverse>(false),
                ToWorldChunk = GetComponentTypeHandle<LocalToWorld>(true),
                WaypointChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true)
            }.ScheduleParallel(_traversing, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new OnCompletionUpdateJob()
            {
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                TraverseChunk = GetComponentTypeHandle<Traverse>(true),
                WaitChunk = GetComponentTypeHandle<Wait>(false)
            }.ScheduleParallel(_componentRemovedQuery, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }

        [BurstCompile]
        public struct PatrolMovementUpdateJob : IJobChunk
        {
            public ComponentTypeHandle<Movement> MovementChunk;
            public ComponentTypeHandle<Traverse
                > TraverseChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> ToWorldChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Movement> movements = chunk.GetNativeArray(MovementChunk);
                NativeArray<Traverse> Traverses = chunk.GetNativeArray(TraverseChunk);
                NativeArray<LocalToWorld> ToWorlds = chunk.GetNativeArray(ToWorldChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Movement move = movements[i];
                    Traverse traverse = Traverses[i];

                    traverse.StartingDistance = Vector3.Distance(ToWorlds[i].Position, traverse.CurWaypoint.Position);

                    move.SetLocation( traverse.CurWaypoint.Position);

                    movements[i] = move;
                    Traverses[i] = traverse;
                }
            }
        }
        [BurstCompile]
        public struct CheckSkipPoint : IJobChunk
        {
            public ComponentTypeHandle<Traverse> TraverseChunk;
            public ComponentTypeHandle<Movement> MovementChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> ToWorldChunk;
            [ReadOnly] public BufferTypeHandle<TravelWaypointBuffer> WaypointChunk;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Movement> movements = chunk.GetNativeArray(MovementChunk);
                NativeArray<Traverse> Traverses = chunk.GetNativeArray(TraverseChunk);
                NativeArray<LocalToWorld> ToWorlds = chunk.GetNativeArray(ToWorldChunk);
                BufferAccessor<TravelWaypointBuffer> WaypointBuffers = chunk.GetBufferAccessor(WaypointChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Traverse traverse = Traverses[i];
                    //TODO  make threat and Proximity Thresholds a variable of the entity
                    if (traverse.CurWaypoint.InfluenceAtPosition.y > .7 && traverse.CurWaypoint.InfluenceAtPosition.y < .75f)
                    {
                        //Movement move = movements[i];
                        //DynamicBuffer<PatrolWaypointBuffer> waypointBuffer = WaypointBuffers[i];
                        //patrol.WaypointIndex = patrol.WaypointIndex >= patrol.NumberOfWayPoints ?
                        // patrol.WaypointIndex++ :
                        //  patrol.WaypointIndex = 0;

                        //patrol.CurWaypoint = waypointBuffer[patrol.WaypointIndex].WayPoint;
                        //patrol.StartingDistance = Vector3.Distance(ToWorlds[i].Position, patrol.CurWaypoint.Position);

                        //move.TargetLocation = patrol.CurWaypoint.Position;
                        //move.CanMove = true;
                        //move.SetTargetLocation = true;

                        //movements[i] = move;
                        //patrols[i] = patrol;
                    }
                }

            }
        }

        public struct OnCompletionUpdateJob : IJobChunk
        {
            public ComponentTypeHandle<Wait> WaitChunk;
            [ReadOnly] public ComponentTypeHandle<Traverse> TraverseChunk;
            public ComponentTypeHandle<Movement> MovementChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Wait> Waits = chunk.GetNativeArray(WaitChunk);
                NativeArray<Traverse> Traverses = chunk.GetNativeArray(TraverseChunk);
                NativeArray<Movement> Moves = chunk.GetNativeArray(MovementChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Wait wait = Waits[i];
                    Traverse traverse = Traverses[i];
                    Movement move = Moves[i];
                    wait.Timer = wait.StartTime = traverse.CurWaypoint.TimeToWaitatWaypoint;
                    move.CanMove = false;

                    Waits[i] = wait;
                    Moves[i] = move;
                }

            }
        }
    }

}