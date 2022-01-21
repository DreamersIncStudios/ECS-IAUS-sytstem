using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS.Component;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;
using Stats;
using AISenses;
using GameModes.DestroyTheTower.TowerSystem;

namespace IAUS.ECS.Systems
{
    public class UpdateTowerStateSystem : SystemBase
    {

        EntityCommandBufferSystem _entityCommandBufferSystem;
        EntityQuery BasicTowers;
        protected override void OnCreate()
        {
            BasicTowers = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(AttackTargetState)), ComponentType.ReadWrite(typeof(GatherResourcesState)),
                        ComponentType.ReadWrite(typeof(EnemyStats))/*,ComponentType.ReadWrite(typeof(RepairState)), ComponentType.ReadWrite(typeof(SpawnDefendersState))*/
                    }
            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new UpdateGatherResourcesState() {
                StatsChunk = GetComponentTypeHandle<EnemyStats>(true),
                AttackChunk = GetComponentTypeHandle<AttackTargetState>(true),
                GatherChunk = GetComponentTypeHandle<GatherResourcesState>(false),
                TowerDataChunk = GetComponentTypeHandle<TowerData>(true)
            }
            .ScheduleParallel(BasicTowers, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }

    
  }

    public struct UpdateGatherResourcesState : IJobChunk
    {
        public ComponentTypeHandle<GatherResourcesState> GatherChunk;
        [ReadOnly]public ComponentTypeHandle<EnemyStats> StatsChunk;
        [ReadOnly] public ComponentTypeHandle<AttackTargetState> AttackChunk;

        [ReadOnly]public ComponentTypeHandle<TowerData> TowerDataChunk;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<GatherResourcesState> Gathers = chunk.GetNativeArray(GatherChunk);
            NativeArray<AttackTargetState> attacks = chunk.GetNativeArray(AttackChunk);
            NativeArray<EnemyStats> Stats = chunk.GetNativeArray(StatsChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                GatherResourcesState gather = Gathers[i];
                if (gather.stateRef.IsCreated)
                {
                    float attackRatio = attacks[i].HighScoreAttack.AttackTarget.entity == Entity.Null ? 1.0f :attacks[i].HighScoreAttack.AttackDistanceRatio;
                    float totalScore = gather.HealthRatio.Output(Stats[i].HealthRatio) * gather.TargetInRange.Output(attackRatio);
                    gather.TotalScore = gather.Status != ActionStatus.CoolDown ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * gather.mod) * totalScore) : 0.0f;
                }
                Gathers[i] = gather;
            }

        }
    }
}