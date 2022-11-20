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
using Utilities.ReactiveSystem;
using IAUS.ECS.Systems.Reactive;
using Components.MovementSystem;

namespace IAUS.ECS.Systems
{
    [UpdateBefore(typeof(TerrorizeReactor.TerrorizeReactiveSystem))]
    public partial class UpdateTerrorizeState : SystemBase
    {
        private EntityQuery terrorizorEntities;
        private EntityQuery _componentAddedQuery;
        private EntityQuery _terrorize;
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
            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(TerrorizeAreaState)), ComponentType.ReadWrite(typeof(TerrorizeAreaStateTag)),
                    ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer)), ComponentType.ReadOnly(typeof(LocalToWorld))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<TerrorizeAreaStateTag, TerrorizeAreaState, TerrorizeReactor>.StateComponent)) }
            });
            _terrorize = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(TerrorizeAreaState)), ComponentType.ReadWrite(typeof(TerrorizeAreaStateTag)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer))
                , ComponentType.ReadOnly(typeof(LocalToWorld)), ComponentType.ReadOnly(typeof(AIReactiveSystemBase<TerrorizeAreaStateTag, TerrorizeAreaState, TerrorizeReactor>.StateComponent)) }
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
                        float TotalScore =  terror.HealthRatio.Output(healthRatio) * terror.TargetInRange.Output(1.0f);
                        terror.TotalScore = terror.Status != ActionStatus.CoolDown ? Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * terror.mod) * TotalScore) : 0.0f;
                    }
                    terrors[i] = terror;
                }
            }
        }
        public struct FindTargetToTerrorize : IJobChunk
        {
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                throw new System.NotImplementedException();
            }
        }


    }
}