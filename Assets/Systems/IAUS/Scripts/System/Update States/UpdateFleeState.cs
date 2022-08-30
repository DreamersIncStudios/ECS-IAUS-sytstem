using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS.Component;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;
using Stats;
using AISenses;


namespace IAUS.ECS.Systems
{
    [UpdateInGroup(typeof(IAUSUpdateGroup))]
    [UpdateBefore(typeof(IAUSBrainUpdate))]
    public sealed partial class UpdateFleeState : SystemBase
    {
        private EntityQuery DistanceCheck;
        private EntityQuery RetreatScore;
        private EntityQuery CompleteCheck;

        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            DistanceCheck = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)), ComponentType.ReadOnly(typeof(LocalToWorld)),
                 ComponentType.ReadOnly(typeof(AlertLevel))}

            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            RetreatScore = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)), ComponentType.ReadOnly(typeof(EnemyStats)), ComponentType.ReadOnly(typeof(IAUSBrain)) }
            });
            CompleteCheck = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(RetreatCitizen)), ComponentType.ReadOnly(typeof(RetreatActionTag)) }

            });
        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            systemDeps = new GetInfluenceAtPosition<RetreatCitizen>() {
                RetreatChunk = GetComponentTypeHandle<RetreatCitizen>(false),
                TransformChunk = GetComponentTypeHandle<LocalToWorld>(true)
            }.ScheduleSingle(DistanceCheck, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new ScoreStateEnemyRetreat()
            {
                StatsChunk = GetComponentTypeHandle<EnemyStats>(true),
                RetreatChunk = GetComponentTypeHandle<RetreatCitizen>(false),
            }.ScheduleParallel(RetreatScore, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);



            Dependency = systemDeps;
        }

        //TODO Abstract at later Date
        public struct ScoreStateEnemyRetreat : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<EnemyStats> StatsChunk;
            public ComponentTypeHandle<RetreatCitizen> RetreatChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<RetreatCitizen> Retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<EnemyStats> Stats = chunk.GetNativeArray(StatsChunk);


                for (int i = 0; i < chunk.Count; i++)
                {
                    RetreatCitizen retreat = Retreats[i];
                    EnemyStats stats = Stats[i];
                    float TotalScore =
                         retreat.HealthRatio.Output(stats.HealthRatio)
                        /** retreat.ProximityInArea.Output(retreat.GridValueAtPos.x)*/
                        * retreat.ThreatInArea.Output(retreat.GridValueAtPos.y)
                        ;
                    retreat.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * retreat.mod) * TotalScore);
                    Retreats[i] = retreat;
                }
            }
        }

        public struct GetInfluenceAtPosition<RETREAT> : IJobChunk
            where RETREAT : unmanaged, BaseRetreat
        {
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> TransformChunk;
            public ComponentTypeHandle<RETREAT> RetreatChunk; 
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<RETREAT> Retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<LocalToWorld> toWorlds = chunk.GetNativeArray(TransformChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    RETREAT retreat = Retreats[i];
                    retreat.CurPos = toWorlds[i].Position;
                    Retreats[i] = retreat;
                    
                }
            }
        }
    }
        
    
}