using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS2.Component;
using Unity.Burst;
using UnityEngine;
using Stats;

namespace IAUS.ECS2.Systems
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
                    ComponentType.ReadOnly(typeof(CharacterStatComponent)) }
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
                EntityChunk = GetArchetypeChunkEntityType(),
                WaitChunk = GetArchetypeChunkComponentType<Wait>(false),
                ConcurrentBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.ScheduleParallel(WaitUpdate, systemDeps);

            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            systemDeps = new UpdateWaitScore()
            {
                WaitChunk = GetArchetypeChunkComponentType<Wait>(false),
                StatsChunk = GetArchetypeChunkComponentType<CharacterStatComponent>(true)
                
            }.ScheduleParallel(WaitScore, systemDeps);
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            systemDeps.Complete();
            Dependency = systemDeps;
        }

        [BurstCompile]
        public struct TimerJob : IJobChunk
        {
            public float DT;
            public ArchetypeChunkComponentType<Wait> WaitChunk;
            [ReadOnly]public ArchetypeChunkEntityType EntityChunk;
            public EntityCommandBuffer.Concurrent ConcurrentBuffer;
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
        [BurstCompile]
        public struct UpdateWaitScore : IJobChunk
        {
            public ArchetypeChunkComponentType<Wait> WaitChunk;
            [ReadOnly] public ArchetypeChunkComponentType<CharacterStatComponent> StatsChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Wait> Waits = chunk.GetNativeArray(WaitChunk);
                NativeArray<CharacterStatComponent> Stats = chunk.GetNativeArray(StatsChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Wait wait = Waits[i];
                    CharacterStatComponent stats = Stats[i];
                    float TotalScore = wait.TimeLeft.Output(wait.TimePercent) * wait.HealthRatio.Output(stats.HealthRatio);
                    wait.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * wait.mod) * TotalScore);

                    Waits[i] = wait;
                }
            }
        }
    }
}