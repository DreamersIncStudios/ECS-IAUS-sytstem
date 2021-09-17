using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS2.Component;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;
using Stats;

namespace IAUS.ECS2.Systems
{

    public class UpdateAttackStateSystem : SystemBase
    {
        private EntityQuery Melee;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            Melee = GetEntityQuery(new EntityQueryDesc()
            { 
                All= new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTypeInfo)), ComponentType.ReadOnly(typeof(AttackTargetState)),
                 ComponentType.ReadOnly(typeof(CharacterStatComponent))
                }
            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }


        protected override void OnUpdate()
        {

        }
        [BurstCompile]
        struct UpdateAttackState : IJobChunk
        {
           public  ComponentTypeHandle<CharacterStatComponent> CharacterStatChunk;
            public ComponentTypeHandle<AttackTargetState> AttackStateChunk;
            public EntityTypeHandle EntityChunk;
            [NativeDisableParallelForRestriction] public ComponentDataFromEntity<LocalToWorld> LocalTransform;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<CharacterStatComponent> Stats = chunk.GetNativeArray(CharacterStatChunk);
                NativeArray<AttackTargetState> AttackStates = chunk.GetNativeArray(AttackStateChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    AttackTargetState State = AttackStates[i];
                    State.HealthRatio = Stats[i].HealthRatio;
                    State.ManaRatio = Stats[i].ManaRatio;
                    if (State.Target == Entity.Null)
                    {
                        State.DistanceToTarget = 10000000000000;
                    }
                    else {
                        State.DistanceToTarget = Vector3.Distance(LocalTransform[entities[i]].Position, LocalTransform[State.Target].Position); 
                    }
                   
                    AttackStates[i] = State;
                }
            }
        }

        struct ScoreBufferSubStates : IJobChunk
        {
            public BufferTypeHandle<AttackTypeInfo> AttackBuffer;
            public ComponentTypeHandle<AttackTargetState> AttackStateChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<AttackTypeInfo> bufferAccessor = chunk.GetBufferAccessor(AttackBuffer);
                NativeArray<AttackTargetState> AttackStates = chunk.GetNativeArray(AttackStateChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    AttackTargetState State = AttackStates[i];

                    DynamicBuffer<AttackTypeInfo> AttackBuffer = bufferAccessor[i];
                    for (int j = 0; j < AttackBuffer.Length; j++)
                    {
                        AttackTypeInfo ScoreAttack = AttackBuffer[i];

                        ScoreAttack.Score = ScoreAttack.HealthRatio.Output(State.HealthRatio)*
                            ScoreAttack.DistanceToTarget
                            ;
                    }
                }
            }
        }
    }
}