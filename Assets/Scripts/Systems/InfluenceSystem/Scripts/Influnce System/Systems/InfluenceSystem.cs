using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using InfluenceSystem.Component;
using DataStructures.ViliWonka.KDTree;
using System.Collections.Generic;

namespace InfluenceSystem.Systems
{
    public class InfluenceSystem : SystemBase
    {
        private EntityQuery GridPoints;
        private EntityQuery Influencers;

        EntityCommandBufferSystem entityCommandBufferSystem;
       NativeArray<Translation> PermListPosition;
        NativeArray<Entity> GridEntities;
       
        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            GridPoints = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(InfluenceGridPoint)), ComponentType.ReadWrite(typeof(Translation))}
            });

            Influencers = GetEntityQuery(new EntityQueryDesc() 
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Influence)), ComponentType.ReadOnly(typeof(LocalToWorld))}
            });


        }

        protected override void OnUpdate()
        {

            if (PermListPosition.Length == 0)
            {
                PermListPosition = GridPoints.ToComponentDataArray<Translation>(Allocator.Persistent);
                GridEntities = GridPoints.ToEntityArray(Allocator.Persistent);
            }

            if (UnityEngine.Time.frameCount % 130== 1)
            {
            JobHandle systemDeps = Dependency;
            systemDeps = new UpdateInfluenceMap()
            {
                GridEntities = GridEntities,
                Positions = PermListPosition,
                InfluenceChunk = GetComponentTypeHandle<Influence>(true),
                LocalChunk = GetComponentTypeHandle<LocalToWorld>(true),
                Grids = GetComponentDataFromEntity<InfluenceGridPoint>(false)

            }.ScheduleSingle(Influencers, systemDeps);

            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            Dependency = systemDeps;


             }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            GridEntities.Dispose();
            PermListPosition.Dispose();
        }

        struct UpdateInfluenceMap : IJobChunk
        {
            public NativeArray <Entity> GridEntities;
            public NativeArray<Translation> Positions;
            [ReadOnly]public ComponentTypeHandle<Influence> InfluenceChunk;
            [ReadOnly]public ComponentTypeHandle<LocalToWorld> LocalChunk;
            [NativeDisableParallelForRestriction] public ComponentDataFromEntity<InfluenceGridPoint> Grids;
        

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                float3[] points = new float3[Positions.Length];
                for (int i = 0; i < Positions.Length; i++)
                {
                    points[i] = Positions[i].Value;
                    Grids[GridEntities[i]] = new InfluenceGridPoint();
                }


                KDQuery query = new KDQuery();

                KDTree tree = new KDTree(points, 32);
                NativeArray<LocalToWorld> toWorlds = chunk.GetNativeArray(LocalChunk);
                NativeArray<Influence> Influe = chunk.GetNativeArray(InfluenceChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    List<int> results = new List<int>();
                    // spherical query
                    query.Radius(tree, toWorlds[i].Position, Influe[i].Range, results);
                    foreach (int index in results)
                    {
                        InfluenceGridPoint grid = new InfluenceGridPoint();
                        Translation GridPos = Positions[index];

                        float dist = Vector3.Distance(GridPos.Value, toWorlds[i].Position);
                        switch (Influe[i].Level)
                        {
                            case NPCLevel.Leader:
                                switch (Influe[i].faction)
                                {
                                    case Faction.Enemy:
                                        grid.Enemies.Protection += dist > Influe[i].Range ? 2 * Influe[i].InfluenceValue - (Influe[i].InfluenceValue / (float)Influe[i].Range) * dist : Influe[i].InfluenceValue;
                                        grid.PlayerParty.Threat += dist > Influe[i].Range ? 2 * Influe[i].InfluenceValue - (Influe[i].InfluenceValue / (float)Influe[i].Range) * dist : Influe[i].InfluenceValue;
                                        break;
                                    case Faction.Player:
                                        grid.Enemies.Threat += dist > Influe[i].Range ? 2 * Influe[i].InfluenceValue - (Influe[i].InfluenceValue / (float)Influe[i].Range) * dist : Influe[i].InfluenceValue;
                                        grid.PlayerParty.Protection += dist > Influe[i].Range ? 2 * Influe[i].InfluenceValue - (Influe[i].InfluenceValue / (float)Influe[i].Range) * dist : Influe[i].InfluenceValue;
                                        break;
                                }
                                break;

                            case NPCLevel.Grunt:
                                switch (Influe[i].faction)
                                {
                                    case Faction.Enemy:
                                        grid.Enemies.Protection -= dist > Influe[i].Range ? 0 : Influe[i].InfluenceValue;
                                        grid.PlayerParty.Threat += dist > Influe[i].Range ? 2 * Influe[i].InfluenceValue - (Influe[i].InfluenceValue / (float)Influe[i].Range) * dist : Influe[i].InfluenceValue;
                                        break;
                                    case Faction.Player:
                                        grid.PlayerParty.Protection -= dist > Influe[i].Range ? 0 : Influe[i].InfluenceValue;
                                        grid.Enemies.Threat += dist > Influe[i].Range ? 2 * Influe[i].InfluenceValue - (Influe[i].InfluenceValue / (float)Influe[i].Range) * dist : Influe[i].InfluenceValue;
                                        break;
                                }
                                break;

                            case NPCLevel.Object:


                                break;
                        }
                        Grids[GridEntities[index]] = grid;


                    }

                }

            }
        }
     

    }
}