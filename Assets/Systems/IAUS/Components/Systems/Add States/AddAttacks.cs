using IAUS.ECS.Component;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;



namespace IAUS.ECS.Systems
{
    public struct AddAttacks : IJobChunk
    {
        public ComponentTypeHandle<AttackState> AttackChunk;
        public BufferTypeHandle<StateBuffer> StateBufferChunk;
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {

            NativeArray<AttackState> Attacks = chunk.GetNativeArray(ref AttackChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(ref StateBufferChunk);
            for (int i = 0; i < chunk.Count; i++)
            {
                AttackState c1 = Attacks[i];
                DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];

                bool add = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.Attack)
                    {
                        add = false;
                        continue;
                    }

                }
                c1.Status = ActionStatus.Idle;

                if (add)
                {
                    stateBuffer.Add(new StateBuffer(AIStates.Attack));

                }


                Attacks[i] = c1;
            }
        }
    }
}