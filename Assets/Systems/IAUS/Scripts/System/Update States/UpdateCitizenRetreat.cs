using DreamersInc.InflunceMapSystem;
using IAUS.ECS.Component;
using PixelCrushers.LoveHate;
using Stats;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.ECS.Systems
{
    [UpdateInGroup(typeof(IAUSUpdateGroup))]
    [UpdateBefore(typeof(IAUSBrainUpdate))]
    public partial class UpdateCitizenRetreat : SystemBase
    {
        private EntityQuery RetreatScore;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            RetreatScore = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)), ComponentType.ReadOnly(typeof(NPCStats)), ComponentType.ReadOnly(typeof(IAUSBrain)),
                ComponentType.ReadOnly(typeof(LocalToWorld))}
            });


            RetreatScore.SetChangedVersionFilter(
                new ComponentType[] {
                    ComponentType.ReadOnly(typeof(LocalToWorld)),
                });
        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new ScoreStateCitizen()
            {
                StatsChunk = GetComponentTypeHandle<NPCStats>(true),
                RetreatChunk = GetComponentTypeHandle<RetreatCitizen>(false),
            }.ScheduleSingle(RetreatScore, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }


        public struct ScoreStateCitizen : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<NPCStats> StatsChunk;
            public ComponentTypeHandle<RetreatCitizen> RetreatChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<RetreatCitizen> Retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<NPCStats> Stats = chunk.GetNativeArray(StatsChunk);


                for (int i = 0; i < chunk.Count; i++)
                {
                    RetreatCitizen retreat = Retreats[i];
                    NPCStats stats = Stats[i];
                    float TotalScore =
                         retreat.HealthRatio.Output(stats.HealthRatio)
                         * retreat.ProximityInArea.Output(Mathf.Clamp01( retreat.InfluenceValueAtPos.x/retreat.CrowdMin))
                        * retreat.ThreatInArea.Output(Mathf.Clamp01( retreat.InfluenceValueAtPos.y/retreat.ThreatThreshold))
                        * retreat.DistanceFromThreat.Output(1.0f)
                        ;
                    retreat.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * retreat.mod) * TotalScore);
                    Retreats[i] = retreat;
                }
            }
        }


    }
}