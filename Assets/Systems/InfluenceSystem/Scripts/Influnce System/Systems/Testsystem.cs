using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using InfluenceSystem.Component;

namespace InfluenceSystem.Systems
{
    public class Testsystem : SystemBase
    {
        private EntityQuery GridPoints;
        private EntityQuery Samplers;
        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            GridPoints = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(InfluenceGridPoint)), ComponentType.ReadOnly(typeof(Translation)) }
            });

     
            Samplers = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(InfluenceSampler)), ComponentType.ReadOnly(typeof(LocalToWorld)) }
            });
        }
        protected override void OnUpdate()
        {
            if (UnityEngine.Time.frameCount % 120 == 2)
            {
                JobHandle systemDeps = Dependency;

                systemDeps = new GetValue()
                {
                    GridChunk = GetArchetypeChunkComponentType<InfluenceGridPoint>(true),
                    PositionChunk = GetArchetypeChunkComponentType<Translation>(true),
                    Sampler = Samplers.ToComponentDataArray<InfluenceSampler>(Allocator.TempJob),
                    transforms = Samplers.ToComponentDataArray<LocalToWorld>(Allocator.TempJob)
                }.ScheduleParallel(GridPoints, systemDeps);

                entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);


                Dependency = systemDeps;
            }
        }

        [BurstCompile]
        struct GetValue : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkComponentType<InfluenceGridPoint> GridChunk;
            [ReadOnly] public ArchetypeChunkComponentType<Translation> PositionChunk;
            [DeallocateOnJobCompletion] [NativeDisableParallelForRestriction] public NativeArray<InfluenceSampler> Sampler;
            [ReadOnly] [DeallocateOnJobCompletion] [NativeDisableParallelForRestriction] public NativeArray<LocalToWorld> transforms;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<InfluenceGridPoint> Grids = chunk.GetNativeArray(GridChunk);
                NativeArray<Translation> Positions = chunk.GetNativeArray(PositionChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    InfluenceGridPoint grid = Grids[i];
                    for (int j = 0; j < Sampler.Length; j++)
                    {
                        if (Vector3.Distance(Positions[i].Value, transforms[j].Position) < 1)
                        {
                            InfluenceSampler temp = new InfluenceSampler();
                            temp.value = grid.Enemies.Protection;
                            Sampler[j] = temp;
                        }


                    }
                }

            }
        }

    }
}