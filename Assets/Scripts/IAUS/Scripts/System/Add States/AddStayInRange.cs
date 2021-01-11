using Unity.Entities;
using Unity.Burst;
using IAUS.ECS2.Component;
using Unity.Collections;

namespace IAUS.ECS2.Systems
{
    [BurstCompile]
    public struct AddStayInRange : IJobChunk
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;

        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<CharacterHealthConsideration> HealthRatio;

        [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
        public ArchetypeChunkComponentType<StayInRange> StayInRangeChunk;
        public ArchetypeChunkBufferType<StateBuffer> StateBufferChunk;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            NativeArray<StayInRange> Stay = chunk.GetNativeArray(StayInRangeChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(StateBufferChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entities[i];
                StayInRange c1 = Stay[i];
                DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];

                bool add = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.GotoLeader)
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
                        StateName = AIStates.GotoLeader,
                        Status = ActionStatus.Idle
                    });

                    if (!HealthRatio.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent<CharacterHealthConsideration>(chunkIndex, entity);
                    }

                }
                Stay[i] = c1;
            }
        }
    }
}