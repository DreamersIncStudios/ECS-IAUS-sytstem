
using UnityEngine;
using Unity.Entities;
using IAUS.ECS.Component;
using Unity.Collections;
using AISenses;
using DreamersInc.InflunceMapSystem;
using Unity.Jobs;
using Unity.Burst;
using Stats;
using IAUS.ECS.Consideration;

namespace IAUS.ECS.Systems
{
    //TODO Need to add a Can Target Filter 
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(IAUSBrainUpdate))]
    public class UpdateAttackBufferScoresSystem : SystemBase
    {
        EntityCommandBufferSystem _entityCommandBufferSystem;
        private EntityQuery LookingForAttack;
        private EntityQuery UpdateAttackStateScore;

        float interval = .20f;
        bool runUpdate => interval <= 0.0f;

        protected override void OnCreate()
        {
            base.OnCreate();
            LookingForAttack = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTypeInfo)),
                    ComponentType.ReadOnly(typeof(ScanPositionBuffer)),
                    ComponentType.ReadOnly(typeof(InfluenceComponent)),
                    ComponentType.ReadOnly(typeof(EnemyStats)),
                    ComponentType.ReadOnly(typeof(IAUSBrain))}
                ,
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AttackActionTag)) }
            });

            UpdateAttackStateScore = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTypeInfo)),
                    ComponentType.ReadOnly(typeof(ScanPositionBuffer)),
                    ComponentType.ReadOnly(typeof(InfluenceComponent)),
                    ComponentType.ReadOnly(typeof(EnemyStats)),
                    ComponentType.ReadOnly(typeof(IAUSBrain)), ComponentType.ReadOnly(typeof(AttackActionTag))}
            });

            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }
        protected override void OnUpdate()
        {

            if (runUpdate)
            {
                JobHandle systemDeps = Dependency;

                systemDeps = new FindAttackableTarget()
                {
                    AttackBufferChunk = GetBufferTypeHandle<AttackTypeInfo>(false),
                    TargetBufferChunk = GetBufferTypeHandle<ScanPositionBuffer>(true)

                }.ScheduleParallel(LookingForAttack, systemDeps);

                _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

                systemDeps = new ScoreBufferSubStates()
                {
                    AttackBufferChunk = GetBufferTypeHandle<AttackTypeInfo>(false),
                    AttackStateChunk = GetComponentTypeHandle<AttackTargetState>(false),
                    CharacterStatChunk = GetComponentTypeHandle<EnemyStats>(true)
                }.ScheduleParallel(LookingForAttack, systemDeps);
                _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);


                systemDeps = new GetHighAttack()
                {
                    AttackStateChunk = GetComponentTypeHandle<AttackTargetState>(false),
                    AttackBufferChunk = GetBufferTypeHandle<AttackTypeInfo>(false),

                }.ScheduleParallel(LookingForAttack, systemDeps);
                _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
                systemDeps = new UpdateScoreHigh()
                {
                    AttackStateChunk = GetComponentTypeHandle<AttackTargetState>(false),
                    CharacterStatChunk = GetComponentTypeHandle<EnemyStats>(true)
                }.ScheduleParallel(UpdateAttackStateScore, systemDeps);
                _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);


                Dependency = systemDeps;
                interval = .20f;
            }

            else
            {
                interval -= 1 / 60.0f;
            }
        }

        /// <summary>
        /// Find the Optimial attacking Target foreach attack style in entity buffer
        /// </summary>
        struct FindAttackableTarget : IJobChunk
        {
            public BufferTypeHandle<AttackTypeInfo> AttackBufferChunk;
            public BufferTypeHandle<ScanPositionBuffer> TargetBufferChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<AttackTypeInfo> AttackBuffers = chunk.GetBufferAccessor(AttackBufferChunk);
                BufferAccessor<ScanPositionBuffer> TargetBuffers = chunk.GetBufferAccessor(TargetBufferChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    DynamicBuffer<AttackTypeInfo> attackTypeInfos = AttackBuffers[i];
                    DynamicBuffer<ScanPositionBuffer> scanPositionBuffers = TargetBuffers[i];

                    if (TargetBuffers[i].IsEmpty)
                        continue;

                    for (int j = 0; j < attackTypeInfos.Length; j++)
                    {
                        if (attackTypeInfos[j].AttackTarget.entity != Entity.Null)
                        {
                            
                            if (!checkTarget(i, j, TargetBuffers, attackTypeInfos))
                            {
                                updateTarget(i, j, TargetBuffers, attackTypeInfos);
                            }
                        }
                        else
                        {
                            updateTarget(i, j, TargetBuffers, attackTypeInfos);
                        }
                    }

                }

            }

            bool checkTarget(int i, int j, BufferAccessor<ScanPositionBuffer> TargetBuffers, DynamicBuffer<AttackTypeInfo> attackTypeInfos)
            {
                foreach (var item in TargetBuffers[i])
                {
                    if (item.target.entity == attackTypeInfos[j].AttackTarget.entity && item.target.CanSee)
                    {
                        AttackTypeInfo attack = attackTypeInfos[j];
                        attack.AttackTarget = item.target;
                        attackTypeInfos[j] = attack;
                        return true;
                    }
                }
                    return false;
            }

                void updateTarget(int i, int j, BufferAccessor<ScanPositionBuffer> TargetBuffers, DynamicBuffer<AttackTypeInfo> attackTypeInfos) {
                ScanPositionBuffer closestTarget = TargetBuffers[i][0];
                foreach (var item in TargetBuffers[i])
                {
                    if (attackTypeInfos[j].InRangeForAttack(item.target.DistanceTo))
                    {
                        //Todo add influence check 
                        if (closestTarget.target.DistanceTo > item.target.DistanceTo)
                            closestTarget = item;
                    }

                }
                AttackTypeInfo attack = attackTypeInfos[j];

                attack.AttackTarget = closestTarget.target;
                attackTypeInfos[j] = attack;

            }
        }

        struct ScoreBufferSubStates : IJobChunk
        {

            public BufferTypeHandle<AttackTypeInfo> AttackBufferChunk;
            [ReadOnly] public ComponentTypeHandle<EnemyStats> CharacterStatChunk;
            public ComponentTypeHandle<AttackTargetState> AttackStateChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<AttackTypeInfo> bufferAccessor = chunk.GetBufferAccessor(AttackBufferChunk);
                NativeArray<EnemyStats> Stats = chunk.GetNativeArray(CharacterStatChunk);
                NativeArray<AttackTargetState> Attacks = chunk.GetNativeArray(AttackStateChunk);


                for (int i = 0; i < chunk.Count; i++)
                {
                    AttackTargetState state = Attacks[i];
                    //Todo Balanace Response curve for start point 
                    DynamicBuffer<AttackTypeInfo> AttackBuffer = bufferAccessor[i];
                    for (int j = 0; j < AttackBuffer.Length; j++)
                    {
                        AttackTypeInfo ScoreAttack = AttackBuffer[j];
                        if (ScoreAttack.stateRef.IsCreated)
                        {
                            float TotalScore = (!ScoreAttack.HealthRatio.Equals(default(ConsiderationScoringData)) ?
                                 ScoreAttack.HealthRatio.Output(Stats[i].HealthRatio) : 1) *

                                  (ScoreAttack.AttackTarget.entity != Entity.Null ?
                                        (!ScoreAttack.RangeToTarget.Equals(default(ConsiderationScoringData)) ?
                                            ScoreAttack.RangeToTarget.Output(Mathf.Clamp01(ScoreAttack.DistanceToTarget / (float)ScoreAttack.AttackRange))
                                            : 1)
                                        : 0) *

                                   (!ScoreAttack.ManaAmmoAmount.Equals(default(ConsiderationScoringData)) ?
                                 ScoreAttack.ManaAmmoAmount.Output(1) : 1) //Todo Get mama/ammo amount
                                 ;

                            ScoreAttack.Score = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * ScoreAttack.mod) * TotalScore);
                            if (ScoreAttack.AttackTarget.entity == Entity.Null)
                            {
                                state.HighScoreAttack = new AttackTypeInfo();
                            }
                            else if (state.HighScoreAttack.style == ScoreAttack.style || ScoreAttack.Score > state.HighScoreAttack.Score)
                            {
                                state.HighScoreAttack = ScoreAttack;
                            }
                        }
                        AttackBuffer[j] = ScoreAttack;
                    }
                    Attacks[i] = state;

                }
            }
        }

        [BurstCompile]
        public struct GetHighAttack : IJobChunk
        {
            public BufferTypeHandle<AttackTypeInfo> AttackBufferChunk;
            public ComponentTypeHandle<AttackTargetState> AttackStateChunk;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<AttackTargetState> Attacks = chunk.GetNativeArray(AttackStateChunk);
                BufferAccessor<AttackTypeInfo> bufferAccessor = chunk.GetBufferAccessor(AttackBufferChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    AttackTargetState state = Attacks[i];
                    DynamicBuffer<AttackTypeInfo> AttackBuffer = bufferAccessor[i];
                    state.HighScoreAttack = AttackBuffer[0];
                    for (int j = 0; j < AttackBuffer.Length; j++)
                    {
                        if (AttackBuffer[j].Score > state.HighScoreAttack.Score)
                        {
                            state.HighScoreAttack = AttackBuffer[j];
                        }
                    }

                }
            }
        }

       // [BurstCompile]
        public struct UpdateScoreHigh : IJobChunk
        {
            public ComponentTypeHandle<AttackTargetState> AttackStateChunk;
            [ReadOnly] public ComponentTypeHandle<EnemyStats> CharacterStatChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<AttackTargetState> Attacks = chunk.GetNativeArray(AttackStateChunk);
                NativeArray<EnemyStats> Stats = chunk.GetNativeArray(CharacterStatChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    AttackTargetState state = Attacks[i];
                    AttackTypeInfo ScoreAttack = state.HighScoreAttack;
                    if (ScoreAttack.stateRef.IsCreated)
                    {
                        float TotalScore = (!ScoreAttack.HealthRatio.Equals(default(ConsiderationScoringData)) ?
                             ScoreAttack.HealthRatio.Output(Stats[i].HealthRatio) : 1) *

                              (ScoreAttack.AttackTarget.entity != Entity.Null ?
                                    (!ScoreAttack.RangeToTarget.Equals(default(ConsiderationScoringData)) ?
                                        ScoreAttack.RangeToTarget.Output(Mathf.Clamp01(ScoreAttack.DistanceToTarget / (float)ScoreAttack.AttackRange))
                                        : 1)
                                    : 0) *

                               (!ScoreAttack.ManaAmmoAmount.Equals(default(ConsiderationScoringData)) ?
                             ScoreAttack.ManaAmmoAmount.Output(1) : 1) //Todo Get mama/ammo amount
                             ;

                        ScoreAttack.Score = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * ScoreAttack.mod) * TotalScore);

                    }
                    state.HighScoreAttack = ScoreAttack;
                    Attacks[i] = state;

                }
            }
        }

    }

}

