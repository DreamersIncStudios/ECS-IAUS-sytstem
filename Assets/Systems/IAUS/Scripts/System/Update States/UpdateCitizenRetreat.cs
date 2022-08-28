using DreamersInc.InflunceMapSystem;
using IAUS.ECS.Component;
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
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)), ComponentType.ReadOnly(typeof(NPCStats)), ComponentType.ReadOnly(typeof(IAUSBrain)) }
            });
        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new GetInfluenceAtPosition<RetreatCitizen>()
            {
                RetreatChunk = GetComponentTypeHandle<RetreatCitizen>(false),
                TransformChunk = GetComponentTypeHandle<LocalToWorld>(true)
            }.ScheduleSingle(RetreatScore, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new ScoreStateCitizen()
            {
                StatsChunk = GetComponentTypeHandle<NPCStats>(true),
                RetreatChunk = GetComponentTypeHandle<RetreatCitizen>(false),
            }.ScheduleParallel(RetreatScore, systemDeps);
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
                    Debug.Log(InfluenceGridMaster.Instance.grid.GetGridObject(retreat.CurPos).ToString());
                    float TotalScore =
                         retreat.HealthRatio.Output(stats.HealthRatio)
                        * retreat.ProximityInArea.Output(Mathf.Clamp01( retreat.GridValueAtPos.x/retreat.CrowdMin))
                        * retreat.ThreatInArea.Output(Mathf.Clamp01( retreat.GridValueAtPos.y/retreat.ThreatThreshold))
                        * retreat.DistanceFromThreat.Output(1.0f)
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