using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS2.Component;
using Unity.Entities;
using Unity.Burst;
using Components.MovementSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<PatrolActionTag, Patrol, IAUS.ECS2.Systems.Reactive.PatrolTagReactor>.StateComponent))]

namespace IAUS.ECS2.Systems.Reactive
{
    public struct PatrolTagReactor : IComponentReactorTagsForAIStates<PatrolActionTag, Patrol>
    {
        public void ComponentAdded(Entity entity, ref PatrolActionTag newComponent, ref Patrol AIStateCompoment)
        {
          AIStateCompoment.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref Patrol AIStateCompoment, in PatrolActionTag oldComponent)
        {
            if (AIStateCompoment.Complete)
            {
                AIStateCompoment.Status = ActionStatus.Success;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
            }
            else
            {
                AIStateCompoment.Status = ActionStatus.Interrupted;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime*2;

            }
        }

        public void ComponentValueChanged(Entity entity, ref PatrolActionTag newComponent, ref Patrol AIStateCompoment, in PatrolActionTag oldComponent)
        {
            Debug.Log("Change");
        }

        public class PatrolReactiveSystem : AIReactiveSystemBase<PatrolActionTag, Patrol, PatrolTagReactor>
        {
            protected override PatrolTagReactor CreateComponentReactor()
            {
                return new PatrolTagReactor();
            }
        }
    }

    public class PatrolMovement : SystemBase
    {
        private EntityQuery _componentAddedQuery;
        private EntityQuery _componentRemovedQuery;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadWrite(typeof(PatrolActionTag)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(PatrolWaypointBuffer))
                , ComponentType.ReadOnly(typeof(LocalToWorld))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<PatrolActionTag, Patrol, IAUS.ECS2.Systems.Reactive.PatrolTagReactor>.StateComponent)) }
            });
      
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new PatrolMovementUpdateJob() 
            {
                MovementChunk = GetArchetypeChunkComponentType<Movement>(false),
                PatrolChunk = GetArchetypeChunkComponentType<Patrol>(false),
                TagChunk = GetArchetypeChunkComponentType<PatrolActionTag>(true),
                WaypointChunk = GetArchetypeChunkBufferType<PatrolWaypointBuffer>(true),
                ToWorldChunk = GetArchetypeChunkComponentType<LocalToWorld>(true)
            }.ScheduleParallel(_componentAddedQuery, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }

        [BurstCompile]
        public struct PatrolMovementUpdateJob : IJobChunk
        {
            public ArchetypeChunkComponentType<Movement> MovementChunk;
            public ArchetypeChunkComponentType<Patrol> PatrolChunk;
            [ReadOnly]public ArchetypeChunkComponentType<PatrolActionTag> TagChunk;
            [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> ToWorldChunk;
            [ReadOnly]public ArchetypeChunkBufferType<PatrolWaypointBuffer> WaypointChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Movement> movements = chunk.GetNativeArray(MovementChunk);
                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
                NativeArray<PatrolActionTag> Tags = chunk.GetNativeArray(TagChunk);
                NativeArray<LocalToWorld> ToWorlds = chunk.GetNativeArray(ToWorldChunk);
                BufferAccessor<PatrolWaypointBuffer> WaypointBuffers = chunk.GetBufferAccessor(WaypointChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Movement move = movements[i];
                    Patrol patrol = patrols[i];
                    DynamicBuffer<PatrolWaypointBuffer> waypointBuffer = WaypointBuffers[i];
                    if (Tags[i].UpdateWayPoint) {
                        patrol.WaypointIndex++;
                        if (patrol.WaypointIndex > patrol.NumberOfWayPoints)
                            patrol.WaypointIndex = 0;
                        patrol.CurWaypoint = waypointBuffer[patrol.WaypointIndex];
                    }
                    patrol.StartingDistance = Vector3.Distance(ToWorlds[i].Position, patrol.CurWaypoint.point.Position);

                    move.TargetLocation = patrol.CurWaypoint.point.Position;
                    move.CanMove = true;
                    move.SetTargetLocation = true;

                    movements[i] = move;
                    patrols[i] = patrol;
                }
            }
        }


        public struct OnCompletionUpdateJob : IJobChunk
        {
            public ArchetypeChunkComponentType<Wait> WaitChunk;
            [ReadOnly]public ArchetypeChunkComponentType<Patrol> PatrolChunk;
            public ArchetypeChunkComponentType<Movement> MovementChunk;
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
                    wait.Timer = patrol.CurWaypoint.TimeToWaitatWaypoint;
                    move.CanMove = false;
                    
                    
                    
                    Waits[i] = wait;
                    Moves[i] = move;
                }

            }
        }
    }

}