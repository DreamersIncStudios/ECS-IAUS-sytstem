using Unity.Collections;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Jobs;
using IAUS.Core;
using Stats;
using UnityEngine.Rendering;

namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_UpdateConsideration))]
  
    public class ConsiderationSystem : SystemBase
    {

        EntityQuery PatrolDisntaceCheckers;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

       
            PatrolDisntaceCheckers = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]{ ComponentType.ReadWrite(typeof(DistanceToConsideration)), ComponentType.ReadOnly(typeof(LocalToWorld)),
                    ComponentType.ReadOnly(typeof(Patrol)),ComponentType.ReadOnly(typeof(PatrolBuffer))}
            });

        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            systemDeps = new DistanceToPatrolPointCheck()
            {
                DistanceToChunk = GetArchetypeChunkComponentType<DistanceToConsideration>(),
                PatrolChunk = GetArchetypeChunkComponentType<Patrol>(),
                ToWorldChunk = GetArchetypeChunkComponentType<LocalToWorld>(),
                PatrolBufferChunk = GetArchetypeChunkBufferType<PatrolBuffer>()
            }.ScheduleParallel(PatrolDisntaceCheckers, systemDeps);
        

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);


           systemDeps = Entities.ForEach((ref DistanceToConsideration distanceTo, in LocalToWorld toWorld, in FollowCharacter follow) =>
            {

                float distanceRemaining = Vector3.Distance(follow.TargetLocation, toWorld.Position);
                // make .7f a variable 
                if (distanceRemaining < .5f)
                    distanceRemaining = 0.0f;
                distanceTo.Ratio = distanceRemaining / follow.DistanceAtStart;

            }).Schedule(systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            ComponentDataFromEntity<LocalToWorld> Transforms = GetComponentDataFromEntity<LocalToWorld>(true);


            systemDeps= Entities
                .ForEach((ref LeaderConsideration Check, in Party party) =>
                {

                    if (party.Leader == Entity.Null)
                    { Check.score = 1; }
                    else
                        Check.score = 0;

                })
                .Schedule(systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);


            Dependency = systemDeps;

        }

        //Rewrite to be more openended variable
        struct DistanceToPatrolPointCheck : IJobChunk
        {
            public ArchetypeChunkComponentType<DistanceToConsideration> DistanceToChunk;
           [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> ToWorldChunk;
            [ReadOnly] public ArchetypeChunkComponentType<Patrol> PatrolChunk;
            [ReadOnly] public ArchetypeChunkBufferType<PatrolBuffer> PatrolBufferChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<DistanceToConsideration> Consider = chunk.GetNativeArray(DistanceToChunk);
                NativeArray<LocalToWorld> toWorld = chunk.GetNativeArray(ToWorldChunk);
                NativeArray<Patrol> patrol = chunk.GetNativeArray(PatrolChunk);
                BufferAccessor<PatrolBuffer> bufferAccessor = chunk.GetBufferAccessor(PatrolBufferChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    DistanceToConsideration distanceTo = Consider[i];
                    DynamicBuffer<PatrolBuffer> buffer = bufferAccessor[i];

                    float distanceRemaining = Vector3.Distance(buffer[patrol[i].index].WayPoint.Point, toWorld[i].Position);
                    // make .7f a variable 
                    if (distanceRemaining < patrol[i].BufferZone)
                        distanceRemaining = 0.0f;

                    distanceTo.Ratio = distanceRemaining / patrol[i].DistanceAtStart > .1f ? distanceRemaining / patrol[i].DistanceAtStart : 0.0f;
                    Consider[i] = distanceTo;

                }
            }
        }


    }





}
