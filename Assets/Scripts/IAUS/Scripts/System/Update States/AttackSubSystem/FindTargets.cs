using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS2.Component;
using Unity.Collections;
using AISenses;
namespace IAUS.ECS2.Systems
{
    public class FindTargets : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
          
        }

        struct FindTargetJob : IJobChunk
        {
            public BufferTypeHandle<AttackTypeInfo> AttackBuffer;
            public BufferTypeHandle<ScanPositionBuffer> Targets;

            public ComponentTypeHandle<AttackTargetState> AttackStateChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<AttackTypeInfo> bufferAccessor = chunk.GetBufferAccessor(AttackBuffer);
                BufferAccessor<ScanPositionBuffer> targetBufferAccessor = chunk.GetBufferAccessor(Targets);

                NativeArray<AttackTargetState> AttackStates = chunk.GetNativeArray(AttackStateChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    AttackTargetState State = AttackStates[i];

                    DynamicBuffer<AttackTypeInfo> AttackBuffer = bufferAccessor[i];
                    DynamicBuffer<ScanPositionBuffer> targetsInQueue = targetBufferAccessor[i];
                    List<Target> targetsInRange = new List<Target>();

                    for (int j = 0; j < AttackBuffer.Length; j++)
                    {
                        switch (AttackBuffer[j].style) {
                            case AttackStyle.Melee:
                            case AttackStyle.MagicMelee:
                                for (int x = 0; x < targetBufferAccessor.Length; x++)
                                {
                                    if (targetsInQueue[x].target.DistanceTo < AttackBuffer[j].AttackRange) {
                                        targetsInRange.Add(targetsInQueue[x].target);

                                    }
                                }
                                break;
                            case AttackStyle.Range:
                            case AttackStyle.MagicRange:
                                for (int x = 0; x < targetBufferAccessor.Length; x++)
                                {
                                    if (targetsInQueue[x].target.DistanceTo < AttackBuffer[j].AttackRange
                                        &&
                                        targetsInQueue[x].target.DistanceTo > 10
                                        )
                                        {
                                        targetsInRange.Add(targetsInQueue[x].target);
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