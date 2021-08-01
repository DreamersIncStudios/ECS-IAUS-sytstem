using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS2.Component;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;
using Stats;
using Utilities;
using Unity.Mathematics;
using AISenses;

[assembly: RegisterGenericJobType(typeof(IAUS.ECS2.Systems.UpdateFleeState.DistanceToEscapePoint<RetreatCitizen>))]
[assembly: RegisterGenericJobType(typeof(IAUS.ECS2.Systems.UpdateFleeState.CompletionChecker<RetreatCitizen>))]

namespace IAUS.ECS2.Systems
{
    public class UpdateFleeState : SystemBase
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
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(RetreatCitizen)), ComponentType.ReadOnly(typeof(RetreatTag)) }

            });
        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            systemDeps = new DistanceToEscapePoint<RetreatCitizen>() { 
            RetreatChunk = GetComponentTypeHandle<RetreatCitizen>(false),
            TransformChunk = GetComponentTypeHandle<LocalToWorld>(true)
            }.ScheduleParallel(DistanceCheck, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new ScoreStateCitizen() {
                StatsChunk= GetComponentTypeHandle<CharacterStatComponent>(true),
                RetreatChunk = GetComponentTypeHandle<RetreatCitizen>(false),
                AlertChunk = GetComponentTypeHandle<AlertLevel>(false)
            }.ScheduleParallel(RetreatScore, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new CompletionChecker<RetreatCitizen>() { 
                EntityChunk = GetEntityTypeHandle(),
                RetreatChunk = GetComponentTypeHandle<RetreatCitizen>(true),
                Buffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter()
            }.Schedule(CompleteCheck, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }
        [BurstCompile]
        public struct DistanceToEscapePoint<RETREAT> : IJobChunk
           where RETREAT : unmanaged, BaseRetreat
        {
            public ComponentTypeHandle<RETREAT> RetreatChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> TransformChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<RETREAT> Retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<LocalToWorld> toWorlds = chunk.GetNativeArray(TransformChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    RETREAT retreat = Retreats[i];
                    if (retreat.EscapePoint.Equals( Unity.Mathematics.float3.zero))
                        return;

                    retreat.distanceToPoint = Vector3.Distance(retreat.EscapePoint, toWorlds[i].Position);
                    Retreats[i] = retreat;
                }
            }
        }



        [BurstCompile]
        public struct ScoreStateCitizen : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<CharacterStatComponent> StatsChunk;
            [ReadOnly] public ComponentTypeHandle<AlertLevel> AlertChunk;
            public ComponentTypeHandle<RetreatCitizen> RetreatChunk;
            
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<RetreatCitizen> Retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<CharacterStatComponent> Stats = chunk.GetNativeArray(StatsChunk);
                NativeArray<AlertLevel> alerts = chunk.GetNativeArray(AlertChunk);


                for (int i = 0; i < chunk.Count; i++)
                {
                    RetreatCitizen retreat = Retreats[i];
                    CharacterStatComponent stats = Stats[i];
                    AlertLevel alert = alerts[i];
                    float TotalScore = retreat.DistanceToSafe.Output(retreat.DistanceRatio) 
                        * retreat.HealthRatio.Output(stats.HealthRatio)
                        * retreat.AlertLevels.Output(alert.Caution/100.0f)
                        ;
                    retreat.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * retreat.mod) * TotalScore);
                    Retreats[i] = retreat;
                }
            }
        }

        [BurstCompile]
        public struct CompletionChecker<RETREAT> : IJobChunk
             where RETREAT : unmanaged, BaseRetreat
        {
            [ReadOnly] public ComponentTypeHandle<RETREAT> RetreatChunk;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public EntityCommandBuffer.ParallelWriter Buffer;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<RETREAT> retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    if (retreats[i].Escaped)
                        Buffer.RemoveComponent<RetreatTag>(chunkIndex, entities[i]);
                }
            }
        }

    }

    [UpdateAfter(typeof (UpdateFleeState))]
    public class UpdateFleeState2 : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref RetreatCitizen retreat, ref LocalToWorld transfom, ref AlertLevel alertLevel) => {
                if (alertLevel.NeedForAlarm && !retreat.HasEscapePoint)
                {
                    float3 safePosition = new float3();
                    if (GlobalFunctions.RandomPoint(alertLevel.DirOfThreat * 35f, 10, result: out safePosition))
                    {
                        retreat.EscapePoint = safePosition;
                        retreat.StartingDistance = Vector3.Distance(transfom.Position, safePosition);
                    }

                }

            });
        }
    }
}