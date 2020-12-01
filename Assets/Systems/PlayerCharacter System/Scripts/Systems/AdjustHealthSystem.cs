using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
namespace Stats
{
    public class AdjustHealthSystem : SystemBase
    {


        EntityQuery _ChangeVitals;


        EntityCommandBufferSystem _entityCommandBufferSystem;
        protected EntityCommandBufferSystem GetCommandBufferSystem()
        {
            return World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        protected override void OnCreate()
        {
            base.OnCreate();
            _ChangeVitals = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(PlayerStatComponent)), ComponentType.ReadWrite(typeof(ChangeVitalBuffer)) }
            });
          
            _entityCommandBufferSystem = GetCommandBufferSystem();
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new IncreaseVitalJob()
            {
                DeltaTime = Time.DeltaTime,
                IncreaseChunk = GetArchetypeChunkBufferType<ChangeVitalBuffer>(false),
                StatsChunk = GetArchetypeChunkComponentType<PlayerStatComponent>(false),
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(_ChangeVitals, systemDeps);


            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }


    }


    public struct IncreaseVitalJob : IJobChunk
    {
        public ArchetypeChunkComponentType<PlayerStatComponent> StatsChunk;
        public ArchetypeChunkBufferType<ChangeVitalBuffer> IncreaseChunk;
        public float DeltaTime;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<PlayerStatComponent> stats = chunk.GetNativeArray<PlayerStatComponent>(StatsChunk);
            BufferAccessor<ChangeVitalBuffer> VitalChanges = chunk.GetBufferAccessor<ChangeVitalBuffer>(IncreaseChunk);
            for (int i = 0; i < chunk.Count; i++)
            {
                PlayerStatComponent stat = stats[i];
                DynamicBuffer<ChangeVitalBuffer> buffer = VitalChanges[i];
                for (int j = 0; j < buffer.Length; j++)
                {
                    VitalChange change = buffer[j];
                    if (change.Iterations > 0)
                    {
                        if (change.Timer > 0.0f)
                        {
                            change.Timer -= DeltaTime;
                            buffer[j] = change;
                        }
                        else
                        {
                            switch (change.type)
                            {
                                case VitalType.Health:
                                    if (change.Increase)
                                    {
                                        stat.CurHealth += change.value;
                                    }
                                    else
                                        stat.CurHealth -= change.value;
                                    break;
                                case VitalType.Mana:
                                    if (change.Increase)
                                    {
                                        stat.CurMana += change.value;
                                    }
                                    else
                                        stat.CurMana -= change.value;
                                    break;
                            }

                            change.Timer = change.Frequency;
                            change.Iterations--;

                            stats[i] = stat;
                            buffer[j] = change;

                        }
                    }
                    else
                        buffer.RemoveAt(j);

                }

            }
        }

    }
}