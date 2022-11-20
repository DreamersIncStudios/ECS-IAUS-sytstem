using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS.Component;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;
using Stats;
using AISenses;
using DreamersInc.InflunceMapSystem;
using PixelCrushers.LoveHate;

namespace IAUS.ECS.Systems
{
    public partial class UpdateTerrorizeState : SystemBase
    {
        private EntityQuery terrorizorEntities;
        EntityCommandBufferSystem _entityCommandBufferSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            terrorizorEntities = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(TerrorizeAreaState)), ComponentType.ReadOnly(typeof(EnemyStats)),
                    ComponentType.ReadOnly(typeof(IAUSBrain)), ComponentType.ReadOnly(typeof(AttackTargetState))
                }
            });

        }
        protected override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }


        [BurstCompile]
        public struct ScoreTerrorizeState : IJobChunk
        {
            public ComponentTypeHandle<TerrorizeAreaState> TerrorChunk;
            [ReadOnly] public ComponentTypeHandle<AttackTargetState> AttackChunk;
            [ReadOnly] public ComponentTypeHandle<EnemyStats> StatsChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<TerrorizeAreaState> terrors = chunk.GetNativeArray(TerrorChunk);
                NativeArray<AttackTargetState> attacks = chunk.GetNativeArray(AttackChunk);
                NativeArray<EnemyStats> Stats = chunk.GetNativeArray(StatsChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    TerrorizeAreaState terror = terrors[i];
                    if (terror.stateRef.IsCreated)
                    {
                        float attackRatio = attacks[i].HighScoreAttack.AttackTarget.entity == Entity.Null ? 1.0f : attacks[i].HighScoreAttack.AttackDistanceRatio;

                        float healthRatio = Stats[i].HealthRatio;
                        float TotalScore =  terror.HealthRatio.Output(healthRatio) * terror.TargetInRange.Output(attackRatio);
                        terror.TotalScore = terror.Status != ActionStatus.CoolDown ? Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * terror.mod) * TotalScore) : 0.0f;
                    }
                    terrors[i] = terror;
                }
            }
        }
    }
}