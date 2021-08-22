using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS2.Component;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;
using Stats;
namespace IAUS.ECS2.Systems
{

    public class StayInRangeUpdate : SystemBase
    {
        private EntityQuery Followers;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            Followers = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(StayInRange)), ComponentType.ReadOnly(typeof(FollowEntityTag)), ComponentType.ReadOnly(typeof(LocalToWorld)),
                    ComponentType.ReadOnly(typeof(CharacterStatComponent))

                }
            });
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            systemDeps = new CheckDistanceToLeader()
            {
                StayChunk = GetComponentTypeHandle<StayInRange>(false),
                FollowerChunk = GetComponentTypeHandle<FollowEntityTag>(true),
                PositionChunk = GetComponentTypeHandle<LocalToWorld>(true),
                EntityPositions = GetComponentDataFromEntity<LocalToWorld>()
            }.ScheduleParallel(Followers, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new ScoreStayInRangeState()
            {
                StayChunk = GetComponentTypeHandle<StayInRange>(false),
                StatsChunk = GetComponentTypeHandle<CharacterStatComponent>(true)
            }.ScheduleParallel(Followers, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }



        [BurstCompile]
        struct CheckDistanceToLeader : IJobChunk
        {
            public ComponentTypeHandle<StayInRange> StayChunk;
            [ReadOnly] public ComponentTypeHandle<FollowEntityTag> FollowerChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> PositionChunk;
            [ReadOnly] [NativeDisableParallelForRestriction] public ComponentDataFromEntity<LocalToWorld> EntityPositions;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<StayInRange> Stay = chunk.GetNativeArray(StayChunk);
                NativeArray<LocalToWorld> Positions = chunk.GetNativeArray(PositionChunk);
                NativeArray<FollowEntityTag> Followers = chunk.GetNativeArray(FollowerChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    StayInRange stay = Stay[i];

                    if(Followers[i].Leader != Entity.Null)
                    stay.DistanceToLeader = Vector3.Distance(Positions[i].Position, EntityPositions[Followers[i].Leader].Position);

                    Stay[i] = stay;
                }
            }
        }
        [BurstCompile]
        public struct ScoreStayInRangeState : IJobChunk
        {
            public ComponentTypeHandle<StayInRange> StayChunk;
            [ReadOnly] public ComponentTypeHandle<CharacterStatComponent> StatsChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<StayInRange> Stays = chunk.GetNativeArray(StayChunk);
                NativeArray<CharacterStatComponent> Stats = chunk.GetNativeArray(StatsChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    StayInRange stay = Stays[i];
                    CharacterStatComponent stats = Stats[i];
                    float TotalScore = stay.DistanceToLead.Output(stay.DistanceRatio) * stay.HealthRatio.Output(stats.HealthRatio);
                    stay.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * stay.mod) * TotalScore);

                    Stays[i] = stay;

                }

            }
        }
    }
}
