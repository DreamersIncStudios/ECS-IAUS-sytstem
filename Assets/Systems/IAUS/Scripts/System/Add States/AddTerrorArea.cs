using Unity.Entities;
using Unity.Burst;
using IAUS.ECS.Component;
using UnityEngine;
using Unity.Collections;
namespace IAUS.ECS.Systems
{
    public struct AddTerrorArea : IJobChunk
    {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;
        [NativeDisableParallelForRestriction][ReadOnly] public ComponentDataFromEntity<DistanceToConsideration> Distance;
        [ReadOnly] public EntityTypeHandle EntityChunk;
        public ComponentTypeHandle<TerrorizeAreaState> TerrorChunk;
        public BufferTypeHandle<StateBuffer> StateBufferChunk;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            NativeArray<TerrorizeAreaState> terrors = chunk.GetNativeArray(TerrorChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(StateBufferChunk);
            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entities[i];
                TerrorizeAreaState c1 = terrors[i];
                DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];

                bool add = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.Terrorize)
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
                        StateName = AIStates.Terrorize,
                        Status = ActionStatus.Idle
                    });

                    if (!Distance.HasComponent(entity))
                    {
                        entityCommandBuffer.AddComponent<DistanceToConsideration>(chunkIndex, entity);
                    }
                }


                terrors[i] = c1;
            }
        }


    }
}
