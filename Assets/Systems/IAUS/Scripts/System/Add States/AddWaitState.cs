using Unity.Entities;
using Unity.Burst;
using IAUS.ECS.Component;
using UnityEngine;
using Unity.Collections;
namespace IAUS.ECS.Systems
{
    [BurstCompile]
    public struct AddWaitState : IJobChunk
    {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;
        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<DistanceToConsideration> Distance;
        [ReadOnly] public EntityTypeHandle EntityChunk;
        public ComponentTypeHandle<Wait> WaitChunk;
        public BufferTypeHandle<StateBuffer> StateBufferChunk;

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

                    if (!Distance.HasComponent(entity))
                    {
                        entityCommandBuffer.AddComponent<DistanceToConsideration>(chunkIndex, entity);
                    }
                }


                Waits[i] = c1;
            }
            
        }
        
    }

}