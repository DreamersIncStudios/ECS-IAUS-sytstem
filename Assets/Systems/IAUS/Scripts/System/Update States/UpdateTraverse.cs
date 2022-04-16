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

    public partial class UpdateTraverse : SystemBase
    {

        private EntityQuery TraverseScore;
        private EntityQuery DistanceCheckTraverse;
        private EntityQuery CompleteCheckTraverse;

        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            TraverseScore = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Traverse)), ComponentType.ReadOnly(typeof(NPCStats)), ComponentType.ReadOnly(typeof(IAUSBrain)),
                             
                }
            });

            DistanceCheckTraverse = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Traverse)), ComponentType.ReadOnly(typeof(LocalToWorld)) }
            });

            DistanceCheckTraverse.SetChangedVersionFilter(
                new ComponentType[] {
                    ComponentType.ReadOnly(typeof(LocalToWorld)),
                    ComponentType.ReadWrite(typeof(Traverse))
                });
            CompleteCheckTraverse = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Traverse)), ComponentType.ReadOnly(typeof(TraverseActionTag)) }
            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new GetDistanceToNextPoint<Traverse>()
            {
                MoveChunk = GetComponentTypeHandle<Traverse>(false),
                TransformChunk = GetComponentTypeHandle<LocalToWorld>(true)
            }.ScheduleParallel(DistanceCheckTraverse, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new ScoreStateTraverse()
            {
                TraverseChunk = GetComponentTypeHandle<Traverse>(false),
                StatsChunk = GetComponentTypeHandle<NPCStats>(true)
            }.ScheduleParallel(TraverseScore, systemDeps);

            systemDeps = new CompletionChecker<Traverse, TraverseActionTag>()
            {
                MoveChunk = GetComponentTypeHandle<Traverse>(false),
                Buffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                EntityChunk = GetEntityTypeHandle(),
                WaitChunk = GetComponentTypeHandle<Wait>(false),
                WaypointChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true)

            }.Schedule(CompleteCheckTraverse, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            Dependency = systemDeps;


        }



        [BurstCompile]
        public struct ScoreStateTraverse : IJobChunk
        {
            public ComponentTypeHandle<Traverse> TraverseChunk;
            [ReadOnly] public ComponentTypeHandle<NPCStats> StatsChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Traverse> traverses = chunk.GetNativeArray(TraverseChunk);

                NativeArray<NPCStats> Stats = chunk.GetNativeArray(StatsChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Traverse traverse = traverses[i];
                    if (traverse.stateRef.IsCreated)
                    {

                        //float attackRatio = attacks[i].HighScoreAttack.AttackTarget.entity == Entity.Null ? 1.0f : attacks[i].HighScoreAttack.AttackDistanceRatio;

                        float healthRatio = Stats[i].HealthRatio;
                        float TotalScore = traverse.DistanceToPoint.Output(traverse.DistanceRatio) * traverse.HealthRatio.Output(healthRatio);
                        traverse.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * traverse.mod) * TotalScore);
                    }
                    traverses[i] = traverse;
                }
            }
        }


    }
}