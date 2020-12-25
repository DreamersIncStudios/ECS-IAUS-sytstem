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

    public class UpdateMoveToTarget : SystemBase
    {
        private EntityQuery Attackers;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            Attackers = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(MoveToTarget)), ComponentType.ReadOnly(typeof(FollowEntityTag)), ComponentType.ReadOnly(typeof(LocalToWorld)),
                    ComponentType.ReadOnly(typeof(CharacterStatComponent))

                }
            });
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            systemDeps = new CheckDistanceToLeader()
            {
                MoveChunk = GetArchetypeChunkComponentType<MoveToTarget>(false),
                FollowerChunk = GetArchetypeChunkComponentType<FollowEntityTag>(true),
                PositionChunk = GetArchetypeChunkComponentType<LocalToWorld>(true),
                EntityPositions = GetComponentDataFromEntity<LocalToWorld>()
            }.ScheduleParallel(Attackers, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new ScoreMoveState()
            {
                MoveChunk = GetArchetypeChunkComponentType<MoveToTarget>(false),
                StatsChunk = GetArchetypeChunkComponentType<CharacterStatComponent>(true)
            }.ScheduleParallel(Attackers, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }



        [BurstCompile]
        struct CheckDistanceToLeader : IJobChunk
        {
            public ArchetypeChunkComponentType<MoveToTarget> MoveChunk;
            [ReadOnly] public ArchetypeChunkComponentType<FollowEntityTag> FollowerChunk;
            [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> PositionChunk;
            [ReadOnly] [NativeDisableParallelForRestriction] public ComponentDataFromEntity<LocalToWorld> EntityPositions;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<MoveToTarget> Moves = chunk.GetNativeArray(MoveChunk);
                NativeArray<LocalToWorld> Positions = chunk.GetNativeArray(PositionChunk);
                NativeArray<FollowEntityTag> Followers = chunk.GetNativeArray(FollowerChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    MoveToTarget move = Moves[i];

                    move.DistanceToLeader = Vector3.Distance(Positions[i].Position, EntityPositions[Followers[i].Leader].Position);

                   Moves[i] = move;
                }
            }
        }
        [BurstCompile]
        public struct ScoreMoveState : IJobChunk
        {
            public ArchetypeChunkComponentType<MoveToTarget> MoveChunk;
            [ReadOnly] public ArchetypeChunkComponentType<CharacterStatComponent> StatsChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<MoveToTarget> Moves = chunk.GetNativeArray(MoveChunk);
                NativeArray<CharacterStatComponent> Stats = chunk.GetNativeArray(StatsChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    MoveToTarget move = Moves[i];
                    CharacterStatComponent stats = Stats[i];
                    float TotalScore = move.DistanceToLead.Output(move.DistanceRatio) * move.HealthRatio.Output(stats.HealthRatio);
                    move.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * move.mod) * TotalScore);

                    Moves[i] = move;

                }

            }
        }
    }
}
