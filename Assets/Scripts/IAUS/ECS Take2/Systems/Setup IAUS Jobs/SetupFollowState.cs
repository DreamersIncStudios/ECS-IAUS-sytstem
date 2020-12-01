using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using InfluenceMap;

namespace IAUS.ECS2.IAUSSetup
{
    [BurstCompile]
    public struct SetupFollowState : IJobChunk
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;

        [ReadOnly] [NativeDisableParallelForRestriction] 
        public ComponentDataFromEntity<InfluenceValues> Influences;

        [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
        public ArchetypeChunkComponentType<FollowCharacter> FollowChunk;
        public ArchetypeChunkBufferType<StateBuffer> StateBufferChunk;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(StateBufferChunk);
            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entities[i];
                DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];

                bool added = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.FollowTarget)
                    { added = false; }
                }

                if (added)
                {

                    stateBuffer.Add(new StateBuffer()
                    {
                        StateName = AIStates.FollowTarget,
                        Status = ActionStatus.Idle
                    });
                    if (!Influences.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent<InfluenceValues>(chunkIndex, entity);
                    }
                    entityCommandBuffer.AddComponent<getpointTag>(chunkIndex, entity);
                }
            }
        }
    }
}