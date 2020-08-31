
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using InfluenceMap;
namespace IAUS.ECS2.IAUSSetup
{
    [BurstCompile]

    public struct SetupHealSelfViaItemState : IJobChunk
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public ArchetypeChunkComponentType<HealSelfViaItem> HealChunk;
        public ArchetypeChunkBufferType<StateBuffer> StateBufferChunk;
        [ReadOnly] public ArchetypeChunkEntityType EntityChunk;


        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<HealTimerConsideration> HealTimer;
        [NativeDisableParallelForRestriction] public ArchetypeChunkBufferType<InventoryConsiderationBuffer> BufferChunk;
        [ReadOnly]
        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<InfluenceValues> Influences;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            NativeArray<HealSelfViaItem> Heals= chunk.GetNativeArray(HealChunk);
            BufferAccessor<InventoryConsiderationBuffer> BufferAccess = chunk.GetBufferAccessor(BufferChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(StateBufferChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entities[i];
                HealSelfViaItem Heal = Heals[i];
                DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];
                DynamicBuffer<InventoryConsiderationBuffer> buffer = BufferAccess[i];
                bool add = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.Heal_Self_Item)
                    { add = false; }
                    
                }

                if (add)
                {
                    stateBuffer.Add(new StateBuffer()
                    {
                        StateName = AIStates.Heal_Self_Item,
                        Status = ActionStatus.Idle
                    });
                    buffer.Add(new InventoryConsiderationBuffer()
                    {
                        Consider = new InventoryConsidederGeneralItem()
                        {
                            ItemTypeToConsider = Dreamers.InventorySystem.TypeOfGeneralItem.Recovery,
                            MaxConsider = Heal.FullInventoryofItem
                        }
                    }) ;

                    if (!HealTimer.Exists(entity))
                    { 
                        entityCommandBuffer.AddComponent<HealTimerConsideration>(chunkIndex, entity);
                    }

                }
            }
        }
    }
}
