using Unity.Collections;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Jobs;
using IAUS.Core;
using Stats;
using Components.MovementSystem;


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
                    ComponentType.ReadOnly(typeof(Patrol)),ComponentType.ReadOnly(typeof(Movement))}
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
                MovementChunk = GetArchetypeChunkComponentType<Movement>()
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



            Dependency = systemDeps;

        }

        //Rewrite to be more openended variable
        struct DistanceToPatrolPointCheck : IJobChunk
        {
            public ArchetypeChunkComponentType<DistanceToConsideration> DistanceToChunk;
           [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> ToWorldChunk;
            [ReadOnly] public ArchetypeChunkComponentType<Patrol> PatrolChunk;
            [ReadOnly] public ArchetypeChunkComponentType<Movement> MovementChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<DistanceToConsideration> Consider = chunk.GetNativeArray(DistanceToChunk);
                NativeArray<LocalToWorld> toWorld = chunk.GetNativeArray(ToWorldChunk);
                NativeArray<Patrol> patrol = chunk.GetNativeArray(PatrolChunk);
                NativeArray<Movement> Movers= chunk.GetNativeArray(MovementChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    DistanceToConsideration distanceTo = Consider[i];

                    float distanceRemaining = distanceTo.test=  Vector3.Distance(patrol[i].waypointRef, toWorld[i].Position);

                    // make .7f a variable 
                    if (Movers[i].Completed && !Movers[i].CanMove)
                        distanceRemaining = 0.0f;

                    distanceTo.Ratio = distanceRemaining / patrol[i].DistanceAtStart > .1f ? distanceRemaining / patrol[i].DistanceAtStart : 0.0f;
                    Consider[i] = distanceTo;

                }
            }
        }


    }





}
