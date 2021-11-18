
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

            systemDeps = new FindAttackableTarget() {
               AttackBufferChunk = GetBufferTypeHandle<AttackTypeInfo>(false),
               TargetBufferChunk = GetBufferTypeHandle<ScanPositionBuffer>(true)

            }.ScheduleParallel(attackers, systemDeps);

            Dependency = systemDeps;
        }
        /// <summary>
        /// Find the Optimial attacking Target foreach attack style in entity buffer
        /// </summary>
        struct FindAttackableTarget : IJobChunk
        {
            public BufferTypeHandle<AttackTypeInfo> AttackBufferChunk;
            public BufferTypeHandle<ScanPositionBuffer> TargetBufferChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<AttackTypeInfo> AttackBuffers = chunk.GetBufferAccessor(AttackBufferChunk);
                BufferAccessor<ScanPositionBuffer> TargetBuffers = chunk.GetBufferAccessor(TargetBufferChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    DynamicBuffer<AttackTypeInfo> attackTypeInfos = AttackBuffers[i];
                    DynamicBuffer<ScanPositionBuffer> scanPositionBuffers = TargetBuffers[i];


                    for (int j = 0; j < attackTypeInfos.Length; j++)
                    {
                        if (TargetBuffers[i].IsEmpty)
                            return;
                        ScanPositionBuffer closestTarget = TargetBuffers[i][0];
                        foreach (var item in TargetBuffers[i] )
                        {
                            if (attackTypeInfos[j].InRangeForAttack(item.target.DistanceTo))
                            {
                                //Todo add influence check 
                                if (closestTarget.target.DistanceTo > item.target.DistanceTo)
                                    closestTarget = item;
                            }

                        }
                        AttackTypeInfo attack = attackTypeInfos[j];

                       attack.AttackTarget = closestTarget.target;
                        attackTypeInfos[j] = attack;
                    }

                }
            
            }
        }
    }

}