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
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Retreat)), ComponentType.ReadOnly(typeof(LocalToWorld)) }

            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            RetreatScore = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Retreat)), ComponentType.ReadOnly(typeof(CharacterStatComponent)), ComponentType.ReadOnly(typeof(IAUSBrain)) }
            });
            CompleteCheck = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Retreat)), ComponentType.ReadOnly(typeof(FleeActionTag)) }

            });
        }
        protected override void OnUpdate()
        {

        }
        [BurstCompile]
        public struct DistanceToEscapePoint : IJobChunk
        {
            public ComponentTypeHandle<Retreat> RetreatChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> TransformChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Retreat> Retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<LocalToWorld> toWorlds = chunk.GetNativeArray(TransformChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Retreat retreat = Retreats[i];
                    retreat.distanceToPoint = Vector3.Distance(retreat.EscapePoint, toWorlds[i].Position);
                    Retreats[i] = retreat;
                }
            }
        }

        [BurstCompile]
        public struct ScoreState : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<CharacterStatComponent> StatsChunk;
            public ComponentTypeHandle<Retreat> RetreatChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Retreat> Retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<CharacterStatComponent> Stats = chunk.GetNativeArray(StatsChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Retreat retreat = Retreats[i];
                    CharacterStatComponent stats = Stats[i];
                    float TotalScore = retreat.DistanceToSafe.Output(retreat.DistanceRatio) * retreat.HealthRatio.Output(stats.HealthRatio);
                    retreat.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * retreat.mod) * TotalScore);
                    Retreats[i] = retreat;
                }
            }
        }

        [BurstCompile]
        public struct CompletionChecker : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<Retreat> RetreatChunk;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public EntityCommandBuffer.ParallelWriter Buffer;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Retreat> retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    if (retreats[i].Escape)
                        Buffer.RemoveComponent<PatrolActionTag>(chunkIndex, entities[i]);
                }
            }
        }

    }
}