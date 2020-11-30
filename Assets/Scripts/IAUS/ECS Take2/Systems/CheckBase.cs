using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using CharacterAlignmentSystem;
using Unity.Collections;
using IAUS.Core;
namespace IAUS.ECS2 {
    [UpdateInGroup(typeof(IAUS_UpdateState))]

    public class CheckBase : SystemBase
    {
        EntityQuery BasesQuery;
        EntityQuery Checkers;
        protected override void OnCreate()
        {
            base.OnCreate();
            BasesQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Base)), ComponentType.ReadOnly(typeof(LocalToWorld)), ComponentType.ReadOnly(typeof(CharacterAlignment)) }

            });
            Checkers = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadOnly(typeof(CharacterAlignment)),
                    ComponentType.ReadOnly(typeof(LocalToWorld)) }
            }

                );

        } 

        protected override void OnUpdate()
        {
    
            NativeArray<Entity> BaseEntities =BasesQuery.ToEntityArray(Allocator.TempJob);
            if (BaseEntities.Length == 0)
            {
                BaseEntities.Dispose();
                return;
            }
            JobHandle systemDeps = Dependency;

            systemDeps = new CheckBaseJob() {
                PatrolChunk = GetArchetypeChunkComponentType<Patrol>(false),
                AlignmentChunk = GetArchetypeChunkComponentType<CharacterAlignment>(true),
                PositionChunk = GetArchetypeChunkComponentType<LocalToWorld>(true),
                BaseEntities = BaseEntities,
                BasesPositions = BasesQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
                BasesFactions = BasesQuery.ToComponentDataArray<CharacterAlignment>(Allocator.TempJob)


            }.ScheduleParallel(Checkers, systemDeps);

        }


        public struct CheckBaseJob : IJobChunk
        {
            public ArchetypeChunkComponentType<Patrol> PatrolChunk;
            [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> PositionChunk;
            [ReadOnly] public ArchetypeChunkComponentType<CharacterAlignment> AlignmentChunk;
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Entity> BaseEntities;
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<LocalToWorld> BasesPositions;
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<CharacterAlignment> BasesFactions;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Patrol> Patrols = chunk.GetNativeArray(PatrolChunk);
                NativeArray<LocalToWorld> Positions = chunk.GetNativeArray(PositionChunk);
                NativeArray<CharacterAlignment> alignments = chunk.GetNativeArray(AlignmentChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Patrol patrol = Patrols[i];
                    LocalToWorld Pos = Positions[i];
                    CharacterAlignment alignment = alignments[i];

                    if (patrol.HomeEntity != Entity.Null)
                    { return; }
                    Entity ClosestBase = new Entity();
                    float DistanceToClosest = new float();
                    for (int j = 0; j < BaseEntities.Length; j++)
                    {
                        if (alignment.Faction == BasesFactions[j].Faction)
                        {
                            float dist = Vector3.Distance(Pos.Position, BasesPositions[j].Position);
                            if (ClosestBase == Entity.Null)
                            {
                                ClosestBase = BaseEntities[j];
                                DistanceToClosest = dist;
                            }
                            else if (dist < DistanceToClosest)
                            {
                                ClosestBase = BaseEntities[j];
                                DistanceToClosest = dist;
                            }
                        }
                    }
                    if (ClosestBase != Entity.Null)
                        patrol.HomeEntity = ClosestBase;

                    Patrols[i] = patrol;
                }

            }
        }
    }
}