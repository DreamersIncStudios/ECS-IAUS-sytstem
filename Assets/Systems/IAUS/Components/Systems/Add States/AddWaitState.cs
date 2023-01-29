using Unity.Entities;
using Unity.Burst;
using IAUS.ECS.Component;
using UnityEngine;
using Unity.Collections;
using Unity.Burst.Intrinsics;

namespace IAUS.ECS.Systems
{
    [BurstCompile]
    public struct AddWaitState : IJobChunk
    {
        public ComponentTypeHandle<Wait> WaitChunk;
        public BufferTypeHandle<StateBuffer> StateBufferChunk;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Wait> Waits = chunk.GetNativeArray(ref WaitChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(ref StateBufferChunk);
            for (int i = 0; i < chunk.Count; i++)
            {
                Wait c1 = Waits[i];
                DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];

                bool add = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.Wait)
                    { 
                        add = false;
                        continue;
                    }

                }
                c1.Status = ActionStatus.Idle;

                if (add)
                {
                    stateBuffer.Add(new StateBuffer()
                    {
                        StateName = AIStates.Wait,
                        Status = ActionStatus.Idle
                    });

                }


                Waits[i] = c1;
            }
            
        }

     
    }

}