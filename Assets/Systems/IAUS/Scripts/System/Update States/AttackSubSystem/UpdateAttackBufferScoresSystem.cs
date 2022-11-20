
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
using Unity.Transforms;

namespace IAUS.ECS.Systems
{
    //TODO Need to add a Can Target Filter 
    [UpdateInGroup(typeof(IAUSUpdateGroup))]
    [UpdateBefore(typeof(IAUSBrainUpdate))]
    public partial class UpdateAttackBufferScoresSystem : SystemBase
    {
        EntityCommandBufferSystem _entityCommandBufferSystem;
        private EntityQuery LookingForAttack;
        private EntityQuery UpdateAttackStateScore;



        protected override void OnCreate()
        {
            base.OnCreate();
            LookingForAttack = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTypeInfo)),
                    ComponentType.ReadWrite(typeof(AttackTargetState)),
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

      
                JobHandle systemDeps = Dependency;


                systemDeps = new UpdateTargetEntity()
                {
                    AttackBufferChunk = GetBufferTypeHandle<AttackTypeInfo>(false),
                    TargetBufferChunk = GetBufferTypeHandle<ScanPositionBuffer>(true)

                }.ScheduleParallel(LookingForAttack, systemDeps);

                _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

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

            Dependency = systemDeps;
        
        }

        /// <summary>
        /// Find the Optimial attacking Target foreach attack style in entity buffer
        /// </summary>
        [BurstCompile]

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

            void updateTarget(int i, int j, BufferAccessor<ScanPositionBuffer> TargetBuffers, DynamicBuffer<AttackTypeInfo> attackTypeInfos)
            {
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
       // [BurstCompile]
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
                        state.HighScoreAttack = new AttackTypeInfo();
                        if (ScoreAttack.stateRef.IsCreated)
                        {
                            float TotalScore = 0.0f;
                            if (ScoreAttack.AttackTarget.entity == Entity.Null)
                            {
                                ScoreAttack.Score = 0.0f;
                            }
                            else
                            {
                                switch (ScoreAttack.style)
                                {
                                    case AttackStyle.Melee:
                                        Debug.Log("Health" + ScoreAttack.HealthRatio.Output(Stats[i].HealthRatio));
                                        Debug.Log("Range" + ScoreAttack.RangeToTarget.Output(Mathf.Clamp01(ScoreAttack.DistanceToTarget / (float)ScoreAttack.AttackRange)));

                                        TotalScore = ScoreAttack.HealthRatio.Output(Stats[i].HealthRatio ) * ScoreAttack.RangeToTarget.Output(Mathf.Clamp01(ScoreAttack.DistanceToTarget / (float)ScoreAttack.AttackRange));
                                        Debug.Log("Total Score" + TotalScore);
                                        break;
                                }

                                ScoreAttack.Score = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * ScoreAttack.mod) * TotalScore);
                                if ( ScoreAttack.Score > state.HighScoreAttack.Score)
                                {
                                    state.HighScoreAttack = ScoreAttack;
                                }
                            }
                        }
                        AttackBuffer[j] = ScoreAttack;
                    }
                    Attacks[i] = state;

                }
            }
        }
        [BurstCompile]
        struct UpdateTargetEntity : IJobChunk
        {
            public BufferTypeHandle<AttackTypeInfo> AttackBufferChunk;
            public BufferTypeHandle<ScanPositionBuffer> TargetBufferChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<AttackTypeInfo> AttackBuffers = chunk.GetBufferAccessor(AttackBufferChunk);
                BufferAccessor<ScanPositionBuffer> TargetBuffers = chunk.GetBufferAccessor(TargetBufferChunk);

                for (int i = 0; i < chunk.Count; i++) { 
                    DynamicBuffer<AttackTypeInfo> attackTypeInfos = AttackBuffers[i];
                    DynamicBuffer<ScanPositionBuffer> scanPositionBuffers = TargetBuffers[i];
                    for (int j = 0; j < attackTypeInfos.Length; j++)
                    {
                        if (attackTypeInfos[j].AttackTarget.entity == Entity.Null)
                            continue;

                        AttackTypeInfo attack = attackTypeInfos[j];
                        foreach (ScanPositionBuffer scanPositionBuffer in scanPositionBuffers) {
                            if(scanPositionBuffer.target.entity== attack.AttackTarget.entity)
                            {
                                attack.AttackTarget = scanPositionBuffer.target;
                            }
                        }
                    }
                }
            }
        }

      

    }

}

