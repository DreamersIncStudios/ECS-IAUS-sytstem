using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms; 
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using Components.MovementSystem;
using Unity.Burst.Intrinsics;

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
            newComponent.WaitTime = AIStateCompoment.CurWaypoint.TimeToWaitatWaypoint;
        }

        public void ComponentRemoved(Entity entity, ref Patrol AIStateCompoment, in PatrolActionTag oldComponent)
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
    public partial class PatrolMovement : SystemBase
    {
        private EntityQuery _componentAddedQuery;
        private EntityQuery _patrolling;

        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadWrite(typeof(PatrolActionTag)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer))
                , ComponentType.ReadOnly(typeof(WorldTransform))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<PatrolActionTag, Patrol, PatrolTagReactor>.StateComponent)) }
            });
            _patrolling = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadWrite(typeof(PatrolActionTag)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer))
                , ComponentType.ReadOnly(typeof(WorldTransform)),
                    ComponentType.ReadOnly(typeof(AIReactiveSystemBase<PatrolActionTag, Patrol, PatrolTagReactor>.StateComponent)) }
            });
 
            _entityCommandBufferSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new PatrolMovementUpdateJob()
            {
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                PatrolChunk = GetComponentTypeHandle<Patrol>(false),
                ToWorldChunk = GetComponentTypeHandle<WorldTransform>(true)
            }.ScheduleParallel(_componentAddedQuery, systemDeps);

            //systemDeps = new CheckSkipPoint()
            //{
            //    MovementChunk = GetComponentTypeHandle<Movement>(false),
            //    PatrolChunk = GetComponentTypeHandle<Patrol>(false),
            //    ToWorldChunk = GetComponentTypeHandle<LocalToWorld>(true),
            //    WaypointChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true)
            //}.ScheduleParallel(_patrolling, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }

        [BurstCompile]
        public struct PatrolMovementUpdateJob : IJobChunk
        {
            public ComponentTypeHandle<Movement> MovementChunk;
            public ComponentTypeHandle<Patrol> PatrolChunk;
            [ReadOnly] public ComponentTypeHandle<WorldTransform> ToWorldChunk;
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Movement> movements = chunk.GetNativeArray(ref MovementChunk);
                NativeArray<Patrol> patrols = chunk.GetNativeArray(ref PatrolChunk);
                NativeArray<WorldTransform> ToWorlds = chunk.GetNativeArray(ref ToWorldChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Movement move = movements[i];
                    Patrol patrol = patrols[i];

                    patrol.StartingDistance = Vector3.Distance(ToWorlds[i].Position, patrol.CurWaypoint.Position);

                    move.SetLocation(patrol.CurWaypoint.Position);


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


            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Movement> movements = chunk.GetNativeArray(ref MovementChunk);
                NativeArray<Patrol> patrols = chunk.GetNativeArray(ref PatrolChunk);
                NativeArray<LocalToWorld> ToWorlds = chunk.GetNativeArray(ref ToWorldChunk);
                BufferAccessor<TravelWaypointBuffer> WaypointBuffers = chunk.GetBufferAccessor(ref WaypointChunk);

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


    }
}