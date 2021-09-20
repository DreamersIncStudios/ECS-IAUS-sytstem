
using UnityEngine;
using Unity.Entities;
using IAUS.ECS2.Component;
using Unity.Collections;
using AISenses;
using DreamersInc.InflunceMapSystem;
using Unity.Jobs;

namespace IAUS.ECS2.Systems
{
    public class FindTargets : SystemBase
    {
        private EntityQuery attackers;
        EntityCommandBuffer ecb;
        protected override void OnCreate()
        {
            base.OnCreate();
            attackers = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTypeInfo)),
                    ComponentType.ReadOnly(typeof(ScanPositionBuffer)),
                    ComponentType.ReadOnly(typeof(InfluenceComponent)),
                    ComponentType.ReadOnly(typeof(IAUSBrain))}
            }
                );
        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            systemDeps = new FindTargetJob() {
                AttackBuffer = GetBufferTypeHandle<AttackTypeInfo>(false),
                Targets = GetBufferTypeHandle<ScanPositionBuffer>(true),
                IausChunk = GetComponentTypeHandle<IAUSBrain>(true),
                InfluenceChunk = GetComponentTypeHandle<InfluenceComponent>(true)
            }.ScheduleParallel(attackers, systemDeps);

            Dependency = systemDeps;
        }

        struct FindTargetJob : IJobChunk
        {
            public BufferTypeHandle<AttackTypeInfo> AttackBuffer;
            [ReadOnly] public BufferTypeHandle<ScanPositionBuffer> Targets;

           [ReadOnly] public ComponentTypeHandle<InfluenceComponent> InfluenceChunk;
            [ReadOnly] public ComponentTypeHandle<IAUSBrain> IausChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<AttackTypeInfo> bufferAccessor = chunk.GetBufferAccessor(AttackBuffer);
                BufferAccessor<ScanPositionBuffer> targetBufferAccessor = chunk.GetBufferAccessor(Targets);
                NativeArray<InfluenceComponent> influences = chunk.GetNativeArray(InfluenceChunk);
                NativeArray<IAUSBrain> brains = chunk.GetNativeArray(IausChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    if (targetBufferAccessor[i].IsEmpty)
                        return;

                    DynamicBuffer<AttackTypeInfo> AttackBuffer = bufferAccessor[i];
                    DynamicBuffer<ScanPositionBuffer> targetsInQueue = targetBufferAccessor[i];
                    Target meleeTarget = new Target();
                    Target rangeTarget = new Target();

                    for (int j = 0; j < AttackBuffer.Length; j++)
                    {
                        float threatMod =1.0f;
                        float protectionMod = 1.0f;
                        switch (brains[i].Attitude) {
                            case Attitude.Normal:
                                protectionMod= threatMod = 1.10f;
                                break;
                        }

                        switch (AttackBuffer[j].style)
                        {
                            case AttackStyle.Melee:
                            case AttackStyle.MagicMelee:
                                for (int x = 0; x < targetBufferAccessor.Length; x++)
                                {
                                    if (targetsInQueue[x].target.CanSee && targetsInQueue[x].target.DistanceTo < AttackBuffer[j].AttackRange)
                                    {
                                        if (InfluenceGridMaster.Instance.grid.GetGridObject(targetsInQueue[x].target.LastKnownPosition).GetValue(influences[i].faction,true).x
                                            <threatMod*influences[i].Threat) 
                                        {
                                            meleeTarget = targetsInQueue[x].target;
                                        }
                                    }
                                }
                                Debug.Log(meleeTarget.CanSee);

                                break;
                            case AttackStyle.Range:
                            case AttackStyle.MagicRange:
                                for (int x = 0; x < targetBufferAccessor.Length; x++)
                                {
                                    if (targetsInQueue[x].target.DistanceTo < AttackBuffer[j].AttackRange
                                        &&
                                        targetsInQueue[x].target.DistanceTo > 10)
                                    {
                                      //  targetsInRange.Add(targetsInQueue[x].target);
                                    }
                                }
                                break;
                        }
                    
                    }



                }
            }
        }
    }

}