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
    public sealed class UpdateFleeState : SystemBase
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
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)), ComponentType.ReadOnly(typeof(CharacterStatComponent)), ComponentType.ReadOnly(typeof(IAUSBrain)) }
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

            systemDeps = new ScoreStateCitizen()
            {
                StatsChunk = GetComponentTypeHandle<CharacterStatComponent>(true),
                RetreatChunk = GetComponentTypeHandle<RetreatCitizen>(false),
            }.ScheduleParallel(RetreatScore, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);



            Dependency = systemDeps;
        }


        public struct ScoreStateCitizen : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<CharacterStatComponent> StatsChunk;
            public ComponentTypeHandle<RetreatCitizen> RetreatChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<RetreatCitizen> Retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<CharacterStatComponent> Stats = chunk.GetNativeArray(StatsChunk);


                for (int i = 0; i < chunk.Count; i++)
                {
                    RetreatCitizen retreat = Retreats[i];
                    CharacterStatComponent stats = Stats[i];
                    float TotalScore =
                         retreat.HealthRatio.Output(stats.HealthRatio)
                        * retreat.ProximityInArea.Output(retreat.GridValueAtPos.x)
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