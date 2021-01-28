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

    public class UpdateAttackMeleeState : SystemBase
    {
        private EntityQuery Melee;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            Melee = GetEntityQuery(new EntityQueryDesc()
            { 
                All= new ComponentType[] { ComponentType.ReadWrite(typeof(MeleeAttackTarget)), ComponentType.ReadOnly(typeof(AttackInfo)),
                 ComponentType.ReadOnly(typeof(CharacterStatComponent))
                }
            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }


        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            systemDeps = new UpdateAttack()
            {
                MeleeChunk = GetComponentTypeHandle<MeleeAttackTarget>(false),
                AttackChunk = GetComponentTypeHandle<AttackInfo>(true),
                StatsChunk = GetComponentTypeHandle<CharacterStatComponent>(true),
                DT = Time.DeltaTime
            }.ScheduleParallel(Melee, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }

        [BurstCompile]
        struct UpdateAttack : IJobChunk
        {
            public ComponentTypeHandle<MeleeAttackTarget> MeleeChunk;
            [ReadOnly]public ComponentTypeHandle<AttackInfo> AttackChunk;
            [ReadOnly] public ComponentTypeHandle<CharacterStatComponent> StatsChunk;

            public float DT;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<MeleeAttackTarget> Melees = chunk.GetNativeArray(MeleeChunk);
                NativeArray<AttackInfo> attackInfos = chunk.GetNativeArray(AttackChunk);
                NativeArray<CharacterStatComponent> Stats = chunk.GetNativeArray(StatsChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    AttackInfo attack = attackInfos[i];
                    MeleeAttackTarget melee = Melees[i];
                    CharacterStatComponent stats = Stats[i];

                    if (!melee.TimeToAttack && attack.InRangeForAttack) {
                        melee.Timer -= DT;
                    }
                    float TotalScore = (melee.TimeToAttack ? 1 : 0) * (attack.InRangeForAttack ? 1 : 0)*melee.HealthRatio.Output(stats.HealthRatio);
                    melee.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * melee.mod) * TotalScore);

                    Melees[i] = melee;
                }

            }
        }
    }
}