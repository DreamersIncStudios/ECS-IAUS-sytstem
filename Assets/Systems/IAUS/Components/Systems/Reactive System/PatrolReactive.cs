﻿using Unity.Collections;
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
    public partial  struct PatrolTagReactor : IComponentReactorTagsForAIStates<PatrolActionTag, Patrol>
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

        public partial class PatrolReactiveSystem : AIReactiveSystemBase<PatrolActionTag, Patrol, PatrolTagReactor>
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
                , ComponentType.ReadOnly(typeof(LocalTransform))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<PatrolActionTag, Patrol, PatrolTagReactor>.StateComponent)) }
            });
            _patrolling = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadWrite(typeof(PatrolActionTag)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer))
                , ComponentType.ReadOnly(typeof(LocalTransform)),
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
                ToWorldChunk = GetComponentTypeHandle<LocalTransform>(true)
            }.ScheduleParallel(_componentAddedQuery, systemDeps);

            //systemDeps = new CheckSkipPoint()
            //{
            //    MovementChunk = GetComponentTypeHandle<Movement>(false),
            //    PatrolChunk = GetComponentTypeHandle<Patrol>(false),
            //    ToWorldChunk = GetComponentTypeHandle<LocalToWorld>(true),
            //    WaypointChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true)
            //}.ScheduleParallel(wanderingStopped , systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }

        [BurstCompile]
        public struct PatrolMovementUpdateJob : IJobChunk
        {
            public ComponentTypeHandle<Movement> MovementChunk;
            public ComponentTypeHandle<Patrol> PatrolChunk;
            [ReadOnly] public ComponentTypeHandle<LocalTransform> ToWorldChunk;
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Movement> movements = chunk.GetNativeArray(ref MovementChunk);
                NativeArray<Patrol> patrols = chunk.GetNativeArray(ref PatrolChunk);
                NativeArray<LocalTransform> ToWorlds = chunk.GetNativeArray(ref ToWorldChunk);
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
                        //escape.WaypointIndex = escape.WaypointIndex >= escape.NumberOfWayPoints ?
                        // escape.WaypointIndex++ :
                        //  escape.WaypointIndex = 0;

                        //escape.CurWaypoint = waypointBuffer[escape.WaypointIndex].WayPoint;
                        //escape.StartingDistance = Vector3.Distance(ToWorlds[i].Position, escape.CurWaypoint.Position);

                        //move.TargetLocation = escape.CurWaypoint.Position;
                        //move.CanMove = true;
                        //move.SetTargetLocation = true;

                        //movements[i] = move;
                        //patrols[i] = escape;
                    }
                }

            }
        }


    }
}