using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS.Component;
using Unity.Burst;
using UnityEngine;
using Stats;
using AISenses;
using IAUS.ECS.Systems.Reactive;
using Unity.Physics;

namespace IAUS.ECS.Systems
{
    [UpdateBefore(typeof(TerrorizeReactor.TerrorizeReactiveSystem))]
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
                    ComponentType.ReadOnly(typeof(IAUSBrain)),
                }
            });
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new FindClosestTarget()
            {
                EnemyChunk = GetBufferTypeHandle<ScanPositionBuffer>(true),
                TerrorChunk = GetComponentTypeHandle<TerrorizeAreaState>(false)
            }.Schedule(terrorizorEntities, systemDeps);
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            systemDeps = new ScoreTerrorizeState()
            {
                StatsChunk = GetComponentTypeHandle<EnemyStats>(true),
                TerrorChunk = GetComponentTypeHandle<TerrorizeAreaState>(false)
            }.Schedule(terrorizorEntities, systemDeps);
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();



            Dependency = systemDeps;
        }

        [BurstCompile]
        public struct FindClosestTarget : IJobChunk
        {
            [ReadOnly] public BufferTypeHandle<ScanPositionBuffer> EnemyChunk;
            public ComponentTypeHandle<TerrorizeAreaState> TerrorChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<TerrorizeAreaState> terrors = chunk.GetNativeArray(TerrorChunk);
                BufferAccessor<ScanPositionBuffer> bufferAccessor = chunk.GetBufferAccessor(EnemyChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    TerrorizeAreaState terror = terrors[i];
                    DynamicBuffer<ScanPositionBuffer> buffer = bufferAccessor[i];
                    if (buffer.IsEmpty)
                    {
                        terror.DistanceToClosestTarget = terror.MaxTerrorizeRadius;
                    }
                    else
                    {
                        if (!terror.HasAttack)
                        {
                            terror.DistanceToClosestTarget = buffer[0].dist;
                            if (buffer.Length > 1)
                            {
                                for (int j = 1; j < buffer.Length; j++)
                                {
                                    if (terror.DistanceToClosestTarget > buffer[j].dist)
                                        terror.DistanceToClosestTarget = buffer[j].dist;
                                }
                            }
                        }
                    }
                    terrors[i] = terror;

                }
            }
        }

      
        [BurstCompile]
        public struct ScoreTerrorizeState : IJobChunk
        {
            public ComponentTypeHandle<TerrorizeAreaState> TerrorChunk;
            [ReadOnly] public ComponentTypeHandle<EnemyStats> StatsChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<TerrorizeAreaState> terrors = chunk.GetNativeArray(TerrorChunk);
                NativeArray<EnemyStats> Stats = chunk.GetNativeArray(StatsChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    TerrorizeAreaState terror = terrors[i];
                    if (terror.stateRef.IsCreated)
                    {
                        float healthRatio = Stats[i].HealthRatio;
                        float TotalScore = terror.HealthRatio.Output(healthRatio) * terror.TargetInRange.Output(terror.targetingRangeInput) *
                                terror.Influence.Output(terror.InfluenceRatio);
                      
                        terror.TotalScore = terror.Status != ActionStatus.CoolDown ? Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * terror.mod) * TotalScore) : 0.0f;
                    }
                    terrors[i] = terror;
                }
            }
        }



    }
}