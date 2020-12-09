using Unity.Entities;
using Unity.Burst;
using IAUS.ECS2.Component;
using UnityEngine;
using Unity.Collections;
namespace IAUS.ECS2.Systems
{
    [BurstCompile]
    public struct AddWaitState : IJobChunk
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<DistanceToConsideration> Distance;
        [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
        public ArchetypeChunkComponentType<Wait> WaitChunk;
        public ArchetypeChunkBufferType<StateBuffer> StateBufferChunk;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            NativeArray<Wait> Waits = chunk.GetNativeArray(WaitChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(StateBufferChunk);
            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entities[i];
                Wait c1 = Waits[i];
                DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];

                bool add = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.Wait)
                    { add = false; }
                }
                c1.Status = ActionStatus.Idle;

                if (add)
                {
                    stateBuffer.Add(new StateBuffer()
                    {
                        StateName = AIStates.Patrol,
                        Status = ActionStatus.Idle
                    });

                    if (!Distance.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent<DistanceToConsideration>(chunkIndex, entity);
                    }
                }


                Waits[i] = c1;
            }
            
        }
        
    }

}