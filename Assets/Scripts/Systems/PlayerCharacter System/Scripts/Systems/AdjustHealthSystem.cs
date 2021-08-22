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
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(CharacterStatComponent)), ComponentType.ReadWrite(typeof(ChangeVitalBuffer)) }
            });
            _ChangeVitals.SetChangedVersionFilter(
                new ComponentType[]
                {
                    ComponentType.ReadWrite(typeof(ChangeVitalBuffer))
                }
                );

            _entityCommandBufferSystem = GetCommandBufferSystem();
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new IncreaseVitalJob()
            {
                DeltaTime = Time.DeltaTime,
                IncreaseChunk = GetBufferTypeHandle<ChangeVitalBuffer>(false),
                StatsChunk = GetComponentTypeHandle<CharacterStatComponent>(false),
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter()
            }.Schedule(_ChangeVitals, systemDeps);


            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }


    }


    public struct IncreaseVitalJob : IJobChunk
    {
        public ComponentTypeHandle<CharacterStatComponent> StatsChunk;
        public BufferTypeHandle<ChangeVitalBuffer> IncreaseChunk;
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<CharacterStatComponent> stats = chunk.GetNativeArray<CharacterStatComponent>(StatsChunk);
            BufferAccessor<ChangeVitalBuffer> VitalChanges = chunk.GetBufferAccessor<ChangeVitalBuffer>(IncreaseChunk);
            for (int i = 0; i < chunk.Count; i++)
            {
                CharacterStatComponent stat = stats[i];
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