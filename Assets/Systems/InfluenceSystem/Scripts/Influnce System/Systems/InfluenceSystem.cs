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
    public class InfluenceSystem : SystemBase
    {
        private EntityQuery GridPoints;
        private EntityQuery Leaders;
        private EntityQuery Grunts;

        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            GridPoints = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(InfluenceGridPoint)), ComponentType.ReadWrite(typeof(Translation))}
            });

            Leaders = GetEntityQuery(new EntityQueryDesc() 
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(LeaderInfluence)), ComponentType.ReadOnly(typeof(LocalToWorld))}
            });

            Grunts = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(GruntInfluence)), ComponentType.ReadOnly(typeof(LocalToWorld)) }
            });
        }
        protected override void OnUpdate()
        {
            if (UnityEngine.Time.frameCount % 120 == 1)
            {
                JobHandle systemDeps = Dependency;
                systemDeps = new UpdateInfluenceMapForLeaders()
                {
                    GridChunk = GetArchetypeChunkComponentType<InfluenceGridPoint>(false),
                    PositionChunk = GetArchetypeChunkComponentType<Translation>(false),
                    Influe = Leaders.ToComponentDataArray<LeaderInfluence>(Allocator.TempJob),
                    transforms = Leaders.ToComponentDataArray<LocalToWorld>(Allocator.TempJob)
                }.ScheduleParallel(GridPoints, systemDeps);
                entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

                systemDeps = new UpdateInfluenceMapForGrunts() 
                {
                    GridChunk = GetArchetypeChunkComponentType<InfluenceGridPoint>(false),
                    PositionChunk = GetArchetypeChunkComponentType<Translation>(true),
                    Influe =Grunts.ToComponentDataArray<GruntInfluence>(Allocator.TempJob),
                    transforms = Grunts.ToComponentDataArray<LocalToWorld>(Allocator.TempJob)
                }.ScheduleParallel(GridPoints, systemDeps);
                entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

                Dependency = systemDeps;
            }
        }

        [BurstCompile]
        struct UpdateInfluenceMapForLeaders : IJobChunk
        {
            public ArchetypeChunkComponentType<InfluenceGridPoint> GridChunk;
            public ArchetypeChunkComponentType<Translation> PositionChunk;
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<LeaderInfluence> Influe;
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<LocalToWorld> transforms;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<InfluenceGridPoint> Grids = chunk.GetNativeArray(GridChunk);
                NativeArray<Translation> Positions = chunk.GetNativeArray(PositionChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    InfluenceGridPoint grid = Grids[i];
                    grid.PlayerParty = new GridValues();
                    grid.Enemies = new GridValues();
                    Translation pos = Positions[i];
                    for (int j = 0; j < Influe.Length; j++)
                    {
                        pos.Value = transforms[j].Position + new float3(grid.GridPoint.x - 50, 0, grid.GridPoint.y - 50);
                        Positions[i] = pos;
                        float dist = Vector3.Distance(pos.Value, transforms[j].Position);
                        if (dist < 2 * Influe[j].Range)
                        {
                            switch (Influe[j].faction)
                            {
                                case Faction.Enemy:
                                    grid.Enemies.Protection += dist > Influe[j].Range  ? 2 * Influe[j].InfluenceValue - (Influe[j].InfluenceValue / (float)Influe[j].Range) * dist : Influe[j].InfluenceValue;
                                    grid.PlayerParty.Threat += dist > Influe[j].Range  ? 2 * Influe[j].InfluenceValue - (Influe[j].InfluenceValue / (float)Influe[j].Range) * dist : Influe[j].InfluenceValue;
                                    break;
                                case Faction.PlayerParty:
                                    grid.Enemies.Threat += dist > Influe[j].Range ? 2 * Influe[j].InfluenceValue - (Influe[j].InfluenceValue / (float)Influe[j].Range) * dist : Influe[j].InfluenceValue;
                                    grid.PlayerParty.Protection += dist > Influe[j].Range ? 2 * Influe[j].InfluenceValue - (Influe[j].InfluenceValue / (float)Influe[j].Range) * dist : Influe[j].InfluenceValue;
                                    break;
                            }
                        }
                    }

                    Grids[i] = grid;
                }

            }
        }
        [BurstCompile]
        struct UpdateInfluenceMapForGrunts : IJobChunk
        {
            public ArchetypeChunkComponentType<InfluenceGridPoint> GridChunk;
            [ReadOnly] public ArchetypeChunkComponentType<Translation> PositionChunk;
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<GruntInfluence> Influe;
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<LocalToWorld> transforms;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<InfluenceGridPoint> Grids = chunk.GetNativeArray(GridChunk);
                NativeArray<Translation> Positions = chunk.GetNativeArray(PositionChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    InfluenceGridPoint grid = Grids[i];
   
                    for (int j = 0; j < Influe.Length; j++)
                    {
                        float dist = Vector3.Distance(Positions[i].Value, transforms[j].Position);
                        if ( dist < 2 * Influe[j].Range)
                        {
                            switch (Influe[j].faction)
                            {
                                case Faction.Enemy:
                                    grid.Enemies.Protection -= dist > Influe[j].Range ? 0 : Influe[j].InfluenceValue;
                                    grid.PlayerParty.Threat += dist > Influe[j].Range ? 2 * Influe[j].InfluenceValue - (Influe[j].InfluenceValue / (float)Influe[j].Range) * dist : Influe[j].InfluenceValue;
                                    break;
                                case Faction.PlayerParty:
                                    grid.PlayerParty.Protection -= dist > Influe[j].Range ? 0 : Influe[j].InfluenceValue;
                                    grid.Enemies.Threat += dist > Influe[j].Range ? 2 * Influe[j].InfluenceValue - (Influe[j].InfluenceValue / (float)Influe[j].Range) * dist : Influe[j].InfluenceValue;
                                    break;
                            }

                        }
                    }
                    Grids[i] = grid;
                }

            }
        }

    }
}