using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS.Component;
using Unity.Burst;
using UnityEngine;
using Stats;

namespace IAUS.ECS.Systems
{
    public class UpdateWait : SystemBase
    {
        private EntityQuery WaitUpdate;
        private EntityQuery WaitScore;

        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            WaitUpdate = GetEntityQuery(new EntityQueryDesc()
            { 
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Wait)), ComponentType.ReadOnly(typeof(WaitActionTag))}
            });
            WaitScore = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Wait)), ComponentType.ReadOnly(typeof(IAUSBrain)), 
                    ComponentType.ReadOnly(typeof(EnemyStats)) }
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

            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            systemDeps = new UpdateWaitScore()
            {
                WaitChunk = GetComponentTypeHandle<Wait>(false),
                StatsChunk = GetComponentTypeHandle<EnemyStats>(true)
                
            }.ScheduleParallel(WaitScore, systemDeps);
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
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

                    if (wait.Timer <= 0.0f)
                    {
                        ConcurrentBuffer.RemoveComponent<WaitActionTag>(chunkIndex, Entities[i]);
                        wait.Timer = 0;
                    }

                    Waits[i] = wait;
                }
            }
        }
        //TODO Abstract Stats at later date 
        [BurstCompile]
        public struct UpdateWaitScore : IJobChunk
        {
            public ComponentTypeHandle<Wait> WaitChunk;
            [ReadOnly] public ComponentTypeHandle<EnemyStats> StatsChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Wait> Waits = chunk.GetNativeArray(WaitChunk);
                NativeArray<EnemyStats> Stats = chunk.GetNativeArray(StatsChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Wait wait = Waits[i];
                    EnemyStats stats = Stats[i];
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