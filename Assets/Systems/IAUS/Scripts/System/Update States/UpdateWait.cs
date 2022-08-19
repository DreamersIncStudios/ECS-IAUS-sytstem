using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS.Component;
using Unity.Burst;
using UnityEngine;
using Stats;

namespace IAUS.ECS.Systems
{
    [UpdateInGroup(typeof(IAUSUpdateGroup))]
    [UpdateBefore(typeof(IAUSBrainUpdate))]
    [UpdateAfter(typeof(UpdatePatrol))]
    [UpdateAfter(typeof(UpdateTraverse))]

    public partial class UpdateWait : SystemBase
    {
        private EntityQuery WaitUpdate;
        private EntityQuery WaitScoreP;
        private EntityQuery WaitScoreT;


        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            WaitUpdate = GetEntityQuery(new EntityQueryDesc()
            { 
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Wait)), ComponentType.ReadOnly(typeof(WaitActionTag)), 
                    ComponentType.ReadOnly(typeof(IAUSBrain)) }
            });
            WaitScoreP = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Wait)), ComponentType.ReadOnly(typeof(IAUSBrain)), 
                    ComponentType.ReadOnly(typeof(EnemyStats)),ComponentType.ReadOnly(typeof(Patrol)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer)) }
            });
            WaitScoreT = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Wait)), ComponentType.ReadOnly(typeof(IAUSBrain)),
                    ComponentType.ReadOnly(typeof(NPCStats)) }
            });
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            float DT = Time.DeltaTime;

            systemDeps = new TimerJob()
            {
                DT = DT,
                EntityChunk = GetEntityTypeHandle(),
                WaitChunk = GetComponentTypeHandle<Wait>(false),
                ConcurrentBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter()
            }.ScheduleParallel(WaitUpdate, systemDeps);
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new TimerComplete<Patrol>() {
                WaitChunk = GetComponentTypeHandle<Wait>(false),
                MoveChunk = GetComponentTypeHandle<Patrol>(false),
                WaypointChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true)

            }.Schedule(WaitScoreP,systemDeps);

            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new TimerComplete<Traverse>()
            {
                WaitChunk = GetComponentTypeHandle<Wait>(false),
                MoveChunk = GetComponentTypeHandle<Traverse>(false),
                WaypointChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true)

            }.Schedule(WaitScoreT, systemDeps);

            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new UpdateWaitScore<EnemyStats>()
            {
                WaitChunk = GetComponentTypeHandle<Wait>(false),
                StatsChunk = GetComponentTypeHandle<EnemyStats>(true)
                
            }.ScheduleParallel(WaitScoreP, systemDeps);
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new UpdateWaitScore<NPCStats>()
            {
                WaitChunk = GetComponentTypeHandle<Wait>(false),
                StatsChunk = GetComponentTypeHandle<NPCStats>(true)

            }.ScheduleParallel(WaitScoreT, systemDeps);
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps.Complete();
            Dependency = systemDeps;
        }

        [BurstCompile]
        public struct TimerJob : IJobChunk
        {
            public float DT;
            public ComponentTypeHandle<Wait> WaitChunk;
            [ReadOnly]public EntityTypeHandle EntityChunk;
            public EntityCommandBuffer.ParallelWriter ConcurrentBuffer;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Wait> Waits = chunk.GetNativeArray(WaitChunk);
                NativeArray<Entity> Entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Wait wait = Waits[i];
                    wait.Timer -= DT;
                    if (wait.Timer <= 0.0f  &&  wait.Timer!=-1.0f)
                    {
                        ConcurrentBuffer.RemoveComponent<WaitActionTag>(chunkIndex, Entities[i]);
                        wait.Timer = -1.0f;
                    }


                    Waits[i] = wait;
                }
            }
        }
        [BurstCompile]
        public struct TimerComplete<T> : IJobChunk
        where T : unmanaged, MovementState

        {
            [ReadOnly] public ComponentTypeHandle<Wait> WaitChunk;
            [ReadOnly] public BufferTypeHandle<TravelWaypointBuffer> WaypointChunk;
            public ComponentTypeHandle<T> MoveChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<T> Moves = chunk.GetNativeArray(MoveChunk);
                NativeArray<Wait> Waits = chunk.GetNativeArray(WaitChunk);
                BufferAccessor<TravelWaypointBuffer> WaypointBuffers = chunk.GetBufferAccessor(WaypointChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    T move = Moves[i];
                    Wait wait = Waits[i];
                    DynamicBuffer<TravelWaypointBuffer> waypointBuffer = WaypointBuffers[i];
                    if (!wait.Complete)
                        continue;
                    else {
                        move.CurWaypoint = waypointBuffer[move.WaypointIndex].WayPoint;
                    }
                    Moves[i] = move;
                
                }


                }

            }

        //TODO Abstract Stats at later date 
        [BurstCompile]
        public struct UpdateWaitScore<STATS> : IJobChunk
            where STATS : unmanaged , StatsComponent
        {
            public ComponentTypeHandle<Wait> WaitChunk;
            [ReadOnly] public ComponentTypeHandle<STATS> StatsChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Wait> Waits = chunk.GetNativeArray(WaitChunk);
                NativeArray<STATS> Stats = chunk.GetNativeArray(StatsChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Wait wait = Waits[i];
                   STATS stats = Stats[i];
                    if (wait.stateRef.IsCreated)
                    {
                        float TotalScore = wait.TimeLeft.Output(wait.TimePercent) * wait.HealthRatio.Output(stats.HealthRatio);
                        wait.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * wait.mod) * TotalScore);
                    }
                    Waits[i] = wait;
                }
            }
        }
    }
}