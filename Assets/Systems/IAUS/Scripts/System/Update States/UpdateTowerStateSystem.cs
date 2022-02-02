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
                        ComponentType.ReadWrite(typeof(EnemyStats)),ComponentType.ReadWrite(typeof(RepairState)), ComponentType.ReadWrite(typeof(SpawnDefendersState))
                    }
            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new UpdateGatherResourcesState()
            {
                StatsChunk = GetComponentTypeHandle<EnemyStats>(true),
                AttackChunk = GetComponentTypeHandle<AttackTargetState>(true),
                GatherChunk = GetComponentTypeHandle<GatherResourcesState>(false),
            }
            .ScheduleParallel(BasicTowers, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            
            systemDeps = new UpdateRepairSelfState() {
                StatsChunk = GetComponentTypeHandle<EnemyStats>(true),
                AttackChunk = GetComponentTypeHandle<AttackTargetState>(true),
                RepairChunk = GetComponentTypeHandle<RepairState>(false),
                TowerDataChunk = GetComponentTypeHandle<TowerData>(true)

            }.ScheduleParallel(BasicTowers, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
          
            systemDeps = new UpdateTowerSpawnDefender()
            {
                StatsChunk = GetComponentTypeHandle<EnemyStats>(true),
                AttackChunk = GetComponentTypeHandle<AttackTargetState>(true),
                SpawnChunk = GetComponentTypeHandle<SpawnDefendersState>(false),
                TowerDataChunk = GetComponentTypeHandle<TowerData>(true)

            }.ScheduleParallel(BasicTowers, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            
            Dependency = systemDeps;
        }


    }
    [BurstCompile]
    public struct UpdateGatherResourcesState : IJobChunk
    {
        public ComponentTypeHandle<GatherResourcesState> GatherChunk;
        [ReadOnly]public ComponentTypeHandle<EnemyStats> StatsChunk;
        [ReadOnly] public ComponentTypeHandle<AttackTargetState> AttackChunk;

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
    [BurstCompile]

    public struct UpdateRepairSelfState : IJobChunk
    {
        public ComponentTypeHandle<RepairState> RepairChunk;
        [ReadOnly] public ComponentTypeHandle<EnemyStats> StatsChunk;
        [ReadOnly] public ComponentTypeHandle<AttackTargetState> AttackChunk;

        [ReadOnly] public ComponentTypeHandle<TowerData> TowerDataChunk;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<RepairState> Repairs = chunk.GetNativeArray(RepairChunk);
            NativeArray<AttackTargetState> attacks = chunk.GetNativeArray(AttackChunk);
            NativeArray<EnemyStats> Stats = chunk.GetNativeArray(StatsChunk);
            NativeArray<TowerData> Energy = chunk.GetNativeArray(TowerDataChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                RepairState repair = Repairs[i];
                if (repair.stateRef.IsCreated)
                {
                    float attackRatio = attacks[i].HighScoreAttack.AttackTarget.entity == Entity.Null ? 1.0f : attacks[i].HighScoreAttack.AttackDistanceRatio;
                    float totalScore = repair.HealthRatio.Output(Stats[i].HealthRatio) * repair.TargetInRange.Output(attackRatio) * repair.EnergyMana.Output(Energy[i].EnergyRatio);
                    repair.TotalScore = repair.Status != ActionStatus.CoolDown ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * repair.mod) * totalScore) : 0.0f;
                }
                Repairs[i] = repair;
            }

        }
    }
    [BurstCompile]
    public struct UpdateTowerSpawnDefender : IJobChunk { 
        [ReadOnly] public ComponentTypeHandle<EnemyStats> StatsChunk;
        public ComponentTypeHandle<SpawnDefendersState> SpawnChunk;
        [ReadOnly] public ComponentTypeHandle<AttackTargetState> AttackChunk;
        [ReadOnly] public ComponentTypeHandle<TowerData> TowerDataChunk;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<SpawnDefendersState> Spawns = chunk.GetNativeArray(SpawnChunk);
            NativeArray<AttackTargetState> attacks = chunk.GetNativeArray(AttackChunk);
            NativeArray<EnemyStats> Stats = chunk.GetNativeArray(StatsChunk);
            NativeArray<TowerData> Energy = chunk.GetNativeArray(TowerDataChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                SpawnDefendersState state = Spawns[i];
                if (state.stateRef.IsCreated)
                {
                    float attackRatio = attacks[i].HighScoreAttack.AttackTarget.entity == Entity.Null ? 1.0f : attacks[i].HighScoreAttack.AttackDistanceRatio;
                    float totalScore = state.HealthRatio.Output(Stats[i].HealthRatio) * state.TargetInRange.Output(attackRatio) * state.EnergyMana.Output(Energy[i].EnergyRatio);
                    state.TotalScore = state.Status != ActionStatus.CoolDown ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * state.mod) * totalScore) : 0.0f;
                }
                Spawns[i] = state;
            }
        }
    }
}