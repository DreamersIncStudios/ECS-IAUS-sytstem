using Unity.Entities;
using Unity.Burst;
using IAUS.ECS.Component;
using Unity.Collections;

namespace IAUS.ECS.Systems
{
    [BurstCompile]
    public struct AddMoveToTargetState : IJobChunk
    {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<CharacterHealthConsideration> HealthRatio;

        [ReadOnly] public EntityTypeHandle EntityChunk;
        public ComponentTypeHandle<MoveToTarget> MoveToTargetChunk;
        public BufferTypeHandle<StateBuffer> StateBufferChunk;

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
                        StateName = AIStates.ChaseMoveToTarget,
                        Status = ActionStatus.Idle
                    });

                    if (!HealthRatio.HasComponent(entity))
                    {
                        entityCommandBuffer.AddComponent<CharacterHealthConsideration>(chunkIndex, entity);
                    }

                }
                Move[i] = c1;
            }
        }
    }
}