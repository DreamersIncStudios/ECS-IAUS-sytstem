﻿using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS2.Component;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;
using Stats;
namespace IAUS.ECS2.Systems
{
    public class UpdatePatrol : SystemBase
    {
        private EntityQuery DistanceCheck;
        private EntityQuery PatrolScore;
        private EntityQuery CompleteCheck;

        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            DistanceCheck = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadOnly(typeof(LocalToWorld)) }
            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            PatrolScore = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadOnly(typeof(CharacterStatComponent)), ComponentType.ReadOnly(typeof(IAUSBrain)) }
            });
           CompleteCheck = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Patrol)), ComponentType.ReadOnly(typeof(PatrolActionTag)) }
            });
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new GetDistanceToNextPoint()
            {
                PatrolChunk = GetArchetypeChunkComponentType<Patrol>(false),
                TransformChunk = GetArchetypeChunkComponentType<LocalToWorld>(true)
            }.ScheduleParallel(DistanceCheck, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            
            systemDeps = new ScoreState() 
            {
                PatrolChunk = GetArchetypeChunkComponentType<Patrol>(),
                StatsChunk = GetArchetypeChunkComponentType<CharacterStatComponent>(true)
            }.ScheduleParallel(PatrolScore, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new CompletionChecker()
            {
                PatrolChunk = GetArchetypeChunkComponentType<Patrol>(true),
                Buffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                EntityChunk = GetArchetypeChunkEntityType()
            }.Schedule(CompleteCheck, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        
        }

        [BurstCompile]
        public struct GetDistanceToNextPoint : IJobChunk
        {
            public ArchetypeChunkComponentType<Patrol> PatrolChunk;
            [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> TransformChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
                NativeArray<LocalToWorld> toWorlds = chunk.GetNativeArray(TransformChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Patrol patrol = patrols[i];
                    LocalToWorld transform = toWorlds[i];
                    patrol.distanceToPoint = Vector3.Distance(patrol.CurWaypoint.Position, transform.Position);
                    if (patrol.InBufferZone)
                        patrol.distanceToPoint = 0.0f;
                    patrols[i] = patrol;
                }

            }
        }

        [BurstCompile]
        public struct ScoreState : IJobChunk
        {
            public ArchetypeChunkComponentType<Patrol> PatrolChunk;
            [ReadOnly]public ArchetypeChunkComponentType<CharacterStatComponent> StatsChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
                NativeArray<CharacterStatComponent> Stats = chunk.GetNativeArray(StatsChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Patrol patrol = patrols[i];
                    CharacterStatComponent stats = Stats[i];
                    float TotalScore = patrol.DistanceToPoint.Output(patrol.DistanceRatio) * patrol.HealthRatio.Output(stats.HealthRatio);
                    patrol.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * patrol.mod) * TotalScore);
                    patrols[i] = patrol;
                }
            }
        }
        [BurstCompile]
        public struct CompletionChecker : IJobChunk
        {
            [ReadOnly]public ArchetypeChunkComponentType<Patrol> PatrolChunk;
            [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
            public EntityCommandBuffer.Concurrent Buffer;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    if (patrols[i].Complete)
                        Buffer.RemoveComponent<PatrolActionTag>(chunkIndex, entities[i]);
                }
            }
        }
    }
}
