using Unity.Entities;
using Unity.Burst;
using IAUS.ECS2.Component;
using Unity.Collections;

namespace IAUS.ECS2.Systems
{
    [BurstCompile]

    public struct AddFleeState : IJobChunk
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;

        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<CharacterHealthConsideration> HealthRatio;
        public ArchetypeChunkComponentType<FleeState> FleeChunk;
        [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
        public ArchetypeChunkBufferType<StateBuffer> StateBufferChunk;



        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            NativeArray<FleeState> Flee = chunk.GetNativeArray(FleeChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(StateBufferChunk);
            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entities[i];
                FleeState c1 = Flee[i];
                DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];

                bool add = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.Retreat)
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
                        StateName = AIStates.Retreat,
                        Status = ActionStatus.Idle
                    });

                    if (!HealthRatio.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent<CharacterHealthConsideration>(chunkIndex, entity);
                    }

                }
                Flee[i] = c1;
            }
            
        
        }
        
    }
}
