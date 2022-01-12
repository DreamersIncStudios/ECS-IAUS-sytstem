using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms; 
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using Components.MovementSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<PatrolActionTag, Patrol, IAUS.ECS.Systems.Reactive.PatrolTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<PatrolActionTag, Patrol, IAUS.ECS.Systems.Reactive.PatrolTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<PatrolActionTag, Patrol, IAUS.ECS.Systems.Reactive.PatrolTagReactor>.ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive
{
    public struct PatrolTagReactor : IComponentReactorTagsForAIStates<PatrolActionTag, Patrol>
    {
        public void ComponentAdded(Entity entity, ref PatrolActionTag newComponent, ref Patrol AIStateCompoment)
        {
          AIStateCompoment.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref Patrol AIStateCompoment, in PatrolActionTag oldComponent)
        {
            if (AIStateCompoment.Complete || AIStateCompoment.Status==ActionStatus.Success)
            {

            }
            else
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime*2;

            }
        }

        public void ComponentValueChanged(Entity entity, ref PatrolActionTag newComponent, ref Patrol AIStateCompoment, in PatrolActionTag oldComponent)
        {
        }

        public class PatrolReactiveSystem : AIReactiveSystemBase<PatrolActionTag, Patrol, PatrolTagReactor>
        {
            protected override PatrolTagReactor CreateComponentReactor()
            {
                return new PatrolTagReactor();
            }
        }
    }
    [UpdateBefore(typeof(PatrolTagReactor.PatrolReactiveSystem))]
    public class PatrolMovement : SystemBase
    {
        private EntityQuery _componentAddedQuery;
        private EntityQuery _componentRemovedQuery;
        private EntityQuery _patrolling;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadWrite(typeof(PatrolActionTag)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer))
                , ComponentType.ReadOnly(typeof(LocalToWorld))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<PatrolActionTag, Patrol, PatrolTagReactor>.StateComponent)) }
            });
            _patrolling = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadWrite(typeof(PatrolActionTag)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer))
                , ComponentType.ReadOnly(typeof(LocalToWorld)),
                    ComponentType.ReadOnly(typeof(AIReactiveSystemBase<PatrolActionTag, Patrol, PatrolTagReactor>.StateComponent)) }
            });
            _componentRemovedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Patrol)), ComponentType.ReadWrite(typeof(Wait)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer)),  ComponentType.ReadWrite(typeof(Movement))
                , ComponentType.ReadOnly(typeof(AIReactiveSystemBase<PatrolActionTag, Patrol, PatrolTagReactor>.StateComponent)),ComponentType.ReadOnly(typeof(LocalToWorld)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(PatrolActionTag))}
            });

            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new PatrolMovementUpdateJob()
            {
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                PatrolChunk = GetComponentTypeHandle<Patrol>(false),
                ToWorldChunk = GetComponentTypeHandle<LocalToWorld>(true)
            }.ScheduleParallel(_componentAddedQuery, systemDeps);

            systemDeps = new CheckSkipPoint() {
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                PatrolChunk = GetComponentTypeHandle<Patrol>(false),
                ToWorldChunk = GetComponentTypeHandle<LocalToWorld>(true),
                WaypointChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true)
            }.ScheduleParallel(_patrolling,systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new OnCompletionUpdateJob()
            {
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                PatrolChunk = GetComponentTypeHandle<Patrol>(true),
                WaitChunk = GetComponentTypeHandle<Wait>(false)
            }.ScheduleParallel(_componentRemovedQuery, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }

        [BurstCompile]
        public struct PatrolMovementUpdateJob : IJobChunk
        {
            public ComponentTypeHandle<Movement> MovementChunk;
            public ComponentTypeHandle<Patrol> PatrolChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> ToWorldChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Movement> movements = chunk.GetNativeArray(MovementChunk);
                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
                NativeArray<LocalToWorld> ToWorlds = chunk.GetNativeArray(ToWorldChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Movement move = movements[i];
                    Patrol patrol = patrols[i];

                    patrol.StartingDistance = Vector3.Distance(ToWorlds[i].Position, patrol.CurWaypoint.Position);

                    move.SetLocation( patrol.CurWaypoint.Position);
                  

                    movements[i] = move;
                    patrols[i] = patrol;
                }
            }
        }
        [BurstCompile]
        public struct CheckSkipPoint : IJobChunk
        {
            public ComponentTypeHandle<Patrol> PatrolChunk;
            public ComponentTypeHandle<Movement> MovementChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> ToWorldChunk;
            [ReadOnly] public BufferTypeHandle<TravelWaypointBuffer> WaypointChunk;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Movement> movements = chunk.GetNativeArray(MovementChunk);
                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
                NativeArray<LocalToWorld> ToWorlds = chunk.GetNativeArray(ToWorldChunk);
                BufferAccessor<TravelWaypointBuffer> WaypointBuffers = chunk.GetBufferAccessor(WaypointChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Patrol patrol = patrols[i];
                    //TODO  make threat and Proximity Thresholds a variable of the entity
                    if (patrol.CurWaypoint.InfluenceAtPosition.y > .7 && patrol.CurWaypoint.InfluenceAtPosition.y < .75f)
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
            [ReadOnly]public ComponentTypeHandle<Patrol> PatrolChunk;
            public ComponentTypeHandle<Movement> MovementChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Wait> Waits = chunk.GetNativeArray(WaitChunk);
                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
                NativeArray<Movement> Moves = chunk.GetNativeArray(MovementChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Wait wait = Waits[i];
                    Patrol patrol = patrols[i];
                    Movement move = Moves[i];
                    if (patrol.Complete) {
                        Debug.Log("works");
                    }

                    wait.Timer=wait.StartTime = patrol.CurWaypoint.TimeToWaitatWaypoint;
                    move.CanMove = false;

                    Waits[i] = wait;
                    Moves[i] = move;
                }

            }
        }
    }

}