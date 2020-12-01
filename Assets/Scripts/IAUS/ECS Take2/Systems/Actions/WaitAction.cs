using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using IAUS.Core;
using Unity.Collections;
using Unity.Burst;

namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_UpdateState))]

    public class WaitAction : SystemBase
    {
        EntityCommandBufferSystem _entityCommandBufferSystem;
        private EntityQuery WaitStateQuery;
        protected override void OnCreate()
        {
            base.OnCreate();
            WaitStateQuery = GetEntityQuery(new EntityQueryDesc() {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(WaitTime)), ComponentType .ReadOnly(typeof(WaitActionTag)), ComponentType.ReadOnly(typeof(BaseAI))}
            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }
        protected override void  OnUpdate()
        {

            JobHandle systemDeps = Dependency;
            systemDeps = new WaitActionJob() {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                DT = Time.DeltaTime,
                WaitStateChunk = GetArchetypeChunkComponentType<WaitTime>(false),
                EntityChunk = GetArchetypeChunkEntityType()
            }
                .ScheduleParallel(WaitStateQuery, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
     
        }
    }
    [BurstCompile]
    public struct WaitActionJob : IJobChunk
    {
        public ArchetypeChunkComponentType<WaitTime> WaitStateChunk;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public float DT;
        [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<WaitTime> Timers = chunk.GetNativeArray(WaitStateChunk);
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            for (int i = 0; i < chunk.Count; i++)
            {
                WaitTime Timer = Timers[i];
                Entity entity = entities[i];
                if (Timer.RemoveTag)
                {
                    entityCommandBuffer.RemoveComponent<WaitActionTag>(chunkIndex, entity);
                    entityCommandBuffer.AddComponent<PatrolUpdateTag>(chunkIndex, entity);

                    Timer.Timer = 0.0f;
                    return;
                }

                //Running
                if (Timer.TimerRunning)
                {
                    Timer.Timer -= DT;
                    Timer.Status = ActionStatus.Running;
                }


                //complete
                if (!Timer.TimerRunning)
                {
                        Timer.Status = ActionStatus.Success;
                }

                Timers[i] = Timer;
            }
        }
    }


}