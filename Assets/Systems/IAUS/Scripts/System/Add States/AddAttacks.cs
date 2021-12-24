using Unity.Entities;
using Unity.Burst;
using IAUS.ECS.Component;
using Unity.Collections;
using UnityEngine;

namespace IAUS.ECS.Systems
{
    public struct AddAttacks : IJobChunk
    {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;
        [ReadOnly] public EntityTypeHandle EntityChunk;
        public BufferTypeHandle<StateBuffer> StateBufferChunk;
        public ComponentTypeHandle<AttackTargetState> AttackChunk;
        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<CharacterHealthConsideration> HealthRatio;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            NativeArray<AttackTargetState> Attacks = chunk.GetNativeArray(AttackChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(StateBufferChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entities[i];
                AttackTargetState c1 = Attacks[i];
                DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];
                bool add = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.Attack)
                    {
                        add = false;
                        continue;
                    }
                }

                if (add)
                {
                    stateBuffer.Add(new StateBuffer()
                    {
                        StateName = AIStates.Attack,
                        Status = ActionStatus.Idle
                    });

                    if (!HealthRatio.HasComponent(entity))
                    {
                        entityCommandBuffer.AddComponent<CharacterHealthConsideration>(chunkIndex, entity);
                    }

                }
                Attacks[i] = c1;
            }

        }
    }
}