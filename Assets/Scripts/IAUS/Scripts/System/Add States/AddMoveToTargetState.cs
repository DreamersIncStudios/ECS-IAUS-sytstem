using Unity.Entities;
using Unity.Burst;
using IAUS.ECS2.Component;
using Unity.Collections;

namespace IAUS.ECS2.Systems
{
    [BurstCompile]
    public struct AddMoveToTargetState : IJobChunk
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;

        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<CharacterHealthConsideration> HealthRatio;

        [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
        public ArchetypeChunkComponentType<MoveToTarget> MoveToTargetChunk;
        public ArchetypeChunkBufferType<StateBuffer> StateBufferChunk;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            NativeArray<MoveToTarget> Move = chunk.GetNativeArray(MoveToTargetChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(StateBufferChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entities[i];
                MoveToTarget c1 = Move[i];
                DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];

                bool add = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.ChaseMoveToTarget)
                    { add = false; }
                }
                c1.Status = ActionStatus.Idle;
                if (add)
                {
                    stateBuffer.Add(new StateBuffer()
                    {
                        StateName = AIStates.ChaseMoveToTarget,
                        Status = ActionStatus.Idle
                    });

                    if (!HealthRatio.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent<CharacterHealthConsideration>(chunkIndex, entity);
                    }

                }
                Move[i] = c1;
            }
        }
    }
}