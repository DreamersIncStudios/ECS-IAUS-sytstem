using Unity.Entities;
using Unity.Collections;
using Unity.Burst;

namespace IAUS.ECS2.IAUSSetup
{
    [BurstCompile]
    public struct SetupWaitState : IJobChunk
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<DistanceToConsideration> Distance;
  

        [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
        public ArchetypeChunkBufferType<StateBuffer> StateBufferChunk;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(StateBufferChunk);
            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entities[i];
                DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];

                bool add = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.Wait)
                    { add = false; }
                }

                if (add)
                {
                    stateBuffer.Add(new StateBuffer()
                    {
                        StateName = AIStates.Wait,
                        Status = ActionStatus.Idle
                    });
              
                    if (!Distance.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent<DistanceToConsideration>(chunkIndex, entity);

                    }
                }
            }
        }
    }
}
