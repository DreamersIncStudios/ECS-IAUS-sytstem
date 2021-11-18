//using Unity.Entities;
//using Unity.Collections;
//using Unity.Jobs;
//using IAUS.ECS.Component;
//using Unity.Burst;
//using Unity.Transforms;
//using UnityEngine;
//using Stats;
//using IAUS.ECS.Consideration;
//using System;

//namespace IAUS.ECS.Systems
//{
//    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
//    [UpdateBefore(typeof(IAUSBrainUpdate))]
//    public class UpdateAttackStateSystem : SystemBase
//    {
//        private EntityQuery Melee;
//        EntityCommandBufferSystem _entityCommandBufferSystem;

//        protected override void OnCreate()
//        {
//            base.OnCreate();
//            Melee = GetEntityQuery(new EntityQueryDesc()
//            { 
//                All= new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTypeInfo)), ComponentType.ReadOnly(typeof(AttackTargetState)),
//                 ComponentType.ReadOnly(typeof(CharacterStatComponent))
//                }
//            });
//            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

//        }
//        float interval = .20f;
//        bool runUpdate => interval <= 0.0f;

//        protected override void OnUpdate()
//        {
//            if (runUpdate)
//            {
//                JobHandle systemDeps = Dependency;
//                systemDeps = new ScoreBufferSubStates()
//                {
//                    AttackBuffer = GetBufferTypeHandle<AttackTypeInfo>(false),
//                    CharacterStatChunk = GetComponentTypeHandle<CharacterStatComponent>(true),
//                    AttackStateChunk = GetComponentTypeHandle<AttackTargetState>(false)
//                }.ScheduleParallel(Melee, systemDeps);
//                _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
//                Dependency = systemDeps;
//                interval = .20f;
//            }
//            else { 
//                interval -= 1 / 60.0f;

//            }
//        }

//        //[BurstCompile]
//        //TODO Check https://forum.unity.com/threads/burst-error-adding-component-frozenrenderscenetag.810753/
//        struct ScoreBufferSubStates : IJobChunk
//        {
//            public BufferTypeHandle<AttackTypeInfo> AttackBuffer;
//           [ReadOnly] public ComponentTypeHandle<CharacterStatComponent> CharacterStatChunk;
//            public ComponentTypeHandle<AttackTargetState> AttackStateChunk;
//            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//            {
//                BufferAccessor<AttackTypeInfo> bufferAccessor = chunk.GetBufferAccessor(AttackBuffer);
//                NativeArray<CharacterStatComponent> Stats = chunk.GetNativeArray(CharacterStatChunk);
//                NativeArray<AttackTargetState> Attacks = chunk.GetNativeArray(AttackStateChunk);


//                for (int i = 0; i < chunk.Count; i++)
//                {
//                    AttackTargetState state = Attacks[i];
//                    state.HighScoreAttack = new AttackTypeInfo();
//                    //Todo Balanace Response curve for start point 
//                    DynamicBuffer<AttackTypeInfo> AttackBuffer = bufferAccessor[i];
//                    for (int j = 0; j < AttackBuffer.Length; j++)
//                    {
//                        AttackTypeInfo ScoreAttack = AttackBuffer[j];
//                        if (ScoreAttack.stateRef.IsCreated)
//                        {
//                            float TotalScore = (!ScoreAttack.HealthRatio.Equals(default(ConsiderationScoringData)) ?
//                                 ScoreAttack.HealthRatio.Output(Stats[i].HealthRatio) : 1) *

//                                 (!ScoreAttack.RangeToTarget.Equals(default(ConsiderationScoringData)) ?
//                                 ScoreAttack.RangeToTarget.Output(Mathf.Clamp01(ScoreAttack.DistanceToTarget/(float)ScoreAttack.AttackRange)) : 2) *

//                                   (!ScoreAttack.ManaAmmoAmount.Equals(default(ConsiderationScoringData)) ?
//                                 ScoreAttack.ManaAmmoAmount.Output(1) : 1) //Todo Get mama/ammo amount
//                                 ;

//                            ScoreAttack.Score = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * ScoreAttack.mod) * TotalScore);

//                            if (ScoreAttack.Score > state.HighScoreAttack.Score)
//                                state.HighScoreAttack = ScoreAttack;
//                        }
//                        AttackBuffer[j] = ScoreAttack;
//                    }
//                    Attacks[i] = state;

//                }
//            }
//        }
//    }
//}