using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS2.Component;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;
namespace IAUS.ECS2.Systems
{
    public class UpdatePatrol : SystemBase
    {
        private EntityQuery DistanceCheck;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            DistanceCheck = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadOnly(typeof(LocalToWorld)) }
            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

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
                    patrol.distanceToPoint = Vector3.Distance(patrol.CurWaypoint.point.Position, transform.Position);

                    patrols[i] = patrol;
                }

            }
        }

        [BurstCompile]
        public struct ScoreState : IJobChunk
        {
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                throw new System.NotImplementedException();
            }
        }

    }
}
