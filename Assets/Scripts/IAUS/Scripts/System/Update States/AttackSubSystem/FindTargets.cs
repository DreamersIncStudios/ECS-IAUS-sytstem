
using UnityEngine;
using Unity.Entities;
using IAUS.ECS.Component;
using Unity.Collections;
using AISenses;
using DreamersInc.InflunceMapSystem;
using DreamersInc.FactionSystem;
using Unity.Jobs;

namespace IAUS.ECS.Systems
{
    //TODO Need to add a Can Target Filter 

    public class FindTargets : SystemBase
    {
        private EntityQuery attackers;
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
                        float threatMod = 1.0f;
                        float protectionMod = 1.0f;
                        switch (brains[i].Attitude)
                        {
                            case Attitude.Normal:
                                protectionMod = threatMod = 1.10f;
                                break;
                        }
                        AttackTypeInfo attack = AttackBuffer[j];
                        switch (attack.style)
                        {
                            case AttackStyle.Melee:
                            case AttackStyle.MagicMelee:
                                for (int x = 0; x < targetsInQueue.Length; x++)
                                {
                                    if (targetsInQueue[x].target.CanSee && targetsInQueue[x].target.DistanceTo < AttackBuffer[j].AttackRange)
                                    {
                                        if (InfluenceGridMaster.Instance.grid.GetGridObject(targetsInQueue[x].target.LastKnownPosition)?.GetValue(FactionManager.Database.GetFaction( influences[i].factionID)).x
                                            < threatMod * influences[i].value.y)
                                        {
                                            meleeTarget = targetsInQueue[x].target;
                                        }
                                    }
                                }
                               attack.AttackTarget = meleeTarget;
                                AttackBuffer[j] = attack;
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