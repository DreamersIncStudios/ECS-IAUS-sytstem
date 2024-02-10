using Unity.Entities;
using IAUS.ECS.Component;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;
using Unity.Burst.Intrinsics;

//TODO Complete Rewrite

namespace IAUS.ECS.Systems
{
    public partial class IAUSBrainSetupSystem : SystemBase
    {
        EntityQuery starter;
        EntityQuery patrolStateEntity;
        EntityQuery wanderStateEntity;

        EntityQuery traverseStateEntity;
        
        protected override void OnCreate()
        {


            patrolStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[] { ComponentType.ReadOnly(typeof(TravelWaypointBuffer)), ComponentType.ReadOnly(typeof(SetupBrainTag)),
                        ComponentType.ReadWrite(typeof(Patrol)) ,ComponentType.ReadOnly(typeof(LocalTransform)) },
            });
            wanderStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[] { ComponentType.ReadOnly(typeof(SetupBrainTag)),
                    ComponentType.ReadWrite(typeof(WanderQuadrant)) ,ComponentType.ReadOnly(typeof(LocalTransform)) },
            });
            traverseStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[] {
                    ComponentType.ReadOnly(typeof(TravelWaypointBuffer)), ComponentType.ReadOnly(typeof(SetupBrainTag)),
                        ComponentType.ReadWrite(typeof(Traverse)) ,ComponentType.ReadOnly(typeof(LocalTransform)) },
            });
       

            starter = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { typeof(IAUSBrain), typeof(SetupBrainTag) }

            });

        }
        protected override void OnUpdate()
        {
            var systemDeps = Dependency;
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();

            systemDeps = new AddMovementState<Patrol>()
            {
                MovementChunk = GetComponentTypeHandle<Patrol>(),
                PatrolBufferChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true),
                ToWorldChunk = GetComponentTypeHandle<LocalTransform>(true)
            }.ScheduleParallel(patrolStateEntity, systemDeps);

            ecbSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new AddMovementState<Traverse>()
            {
           
                MovementChunk = GetComponentTypeHandle<Traverse>(),
                PatrolBufferChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true),
                ToWorldChunk = GetComponentTypeHandle<LocalTransform>(true)
            }.ScheduleParallel(traverseStateEntity, systemDeps);
            ecbSystem.AddJobHandleForProducer(systemDeps);
            
            systemDeps = new AddWanderState() { 
                ECB = ecbSingleton.CreateCommandBuffer(World.Unmanaged).AsParallelWriter()
            }.ScheduleParallel(wanderStateEntity, systemDeps);
            ecbSystem.AddJobHandleForProducer(systemDeps);
          
            // This is to be the last job of this system
            systemDeps = new RemoveSetupTag()
            {
                EntityCommandBuffer = ecbSystem.CreateCommandBuffer().AsParallelWriter(),
                EntityChunk = GetEntityTypeHandle()
            }.ScheduleParallel(starter, systemDeps);
            ecbSystem.AddJobHandleForProducer(systemDeps);

   

            Dependency = systemDeps;
        }


        [BurstCompile]
        public struct RemoveSetupTag : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    EntityCommandBuffer.RemoveComponent<SetupBrainTag>(unfilteredChunkIndex, entities[i]);
                }
            }


        }
    }
}