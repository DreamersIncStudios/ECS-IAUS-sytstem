using Unity.Entities;
using IAUS.ECS2.Component;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
namespace IAUS.ECS2.Systems
{
    public class IAUSBrainSetupSystem : SystemBase
    {
        EntityQuery Starter;
        EntityQuery _partolStateEntity;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            Starter = GetEntityQuery(new EntityQueryDesc() {
                All = new ComponentType[] { typeof(IAUSBrain), typeof(StateBuffer), typeof(SetupBrainTag) }

            });
            _partolStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(PatrolWaypointBuffer)), ComponentType.ReadOnly(typeof(SetupBrainTag)),
                        ComponentType.ReadWrite(typeof(StateBuffer)), ComponentType.ReadWrite(typeof(Patrol)) }

            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;



            // This is to be the last job of this system

            systemDeps = new RemoveSetupTag()
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                EntityChunk = GetArchetypeChunkEntityType()
            }.ScheduleParallel(Starter, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }


        [BurstCompile]
        public struct RemoveSetupTag : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
            public EntityCommandBuffer.Concurrent entityCommandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Entity> Entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    entityCommandBuffer.RemoveComponent<SetupBrainTag>(chunkIndex, Entities[i]);
                }
            }
        }
    }
}