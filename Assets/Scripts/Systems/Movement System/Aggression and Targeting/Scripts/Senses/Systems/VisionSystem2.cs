using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using Unity.Physics;
using Unity.Jobs;
using Unity.Physics.Systems;
using UnityEngine;
using Global.Component;
namespace AISenses.VisionSystems
{
    public class VisionSystem2 : SystemBase
    {
        private EntityQuery SeerEntityQuery;

        private EntityQuery TargetEntityQuery;
        EntityCommandBufferSystem entityCommandBufferSystem;
        EndFramePhysicsSystem m_EndFramePhysicsSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            SeerEntityQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Vision)), ComponentType.ReadOnly(typeof(LocalToWorld)), ComponentType.ReadWrite(typeof(ScanPositionBuffer)) }
            });

            TargetEntityQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(LocalToWorld)), ComponentType.ReadWrite(typeof(AITarget)) }

            });
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
           m_EndFramePhysicsSystem = World.GetExistingSystem<EndFramePhysicsSystem>();

        }

        protected override void OnUpdate()
        {
            CollisionWorld collisionWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;
            Dependency = JobHandle.CombineDependencies(Dependency, m_EndFramePhysicsSystem.GetOutputDependency());
            JobHandle systemDeps = Dependency;
            systemDeps = new AddRaycastBuffer()
            { 
                transformChunk = GetComponentTypeHandle<LocalToWorld>(true),
                VisionChunk = GetComponentTypeHandle<Vision>(true),
                BufferChunk = GetBufferTypeHandle<RaycastBuffer>(false),
                PossibleTargetPosition = TargetEntityQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob)
                
            }.ScheduleParallel(SeerEntityQuery, systemDeps);
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new CastBufferRays() {
                world = collisionWorld,
                BufferChunk = GetBufferTypeHandle<RaycastBuffer>(false)
            }.ScheduleParallel(SeerEntityQuery, systemDeps);
            
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().AddInputDependency(systemDeps);
            Dependency = systemDeps;

        }

        [BurstCompile]
        struct AddRaycastBuffer : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> transformChunk;
            [ReadOnly] public ComponentTypeHandle<Vision> VisionChunk;
            [NativeDisableParallelForRestriction] [DeallocateOnJobCompletion] public NativeArray<LocalToWorld> PossibleTargetPosition;
            public BufferTypeHandle<RaycastBuffer> BufferChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Vision> vision = chunk.GetNativeArray(VisionChunk);
                NativeArray<LocalToWorld> tranforms = chunk.GetNativeArray(transformChunk);
                BufferAccessor<RaycastBuffer> bufferAccessor = chunk.GetBufferAccessor(BufferChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    bufferAccessor[i].Clear();
                    for (int j = 0; j < PossibleTargetPosition.Length; j++)
                    {
                        float dist = Vector3.Distance(tranforms[i].Position, PossibleTargetPosition[j].Position);
                        if (dist <= vision[i].viewRadius)
                        {
                            Vector3 dirToTarget = ((Vector3)PossibleTargetPosition[j].Position - (Vector3)tranforms[i].Position).normalized;
                            if (Vector3.Angle(tranforms[i].Forward, dirToTarget) < vision[i].ViewAngle / 2.0f)
                            {
                                bufferAccessor[i].Add(new RaycastBuffer()
                                {
                                    test = new RaycastInput()
                                    {
                                        Start = tranforms[i].Position,
                                        End = PossibleTargetPosition[j].Position,
                                        Filter = new CollisionFilter
                                        {
                                            BelongsTo = ~0u,
                                            CollidesWith = ((1 << 10) | (1 << 11) | (1 << 12)),
                                            GroupIndex = 0
                                        }
                                    }
                                });

                            }
                        }
                    }

                }
            }
        }

        [BurstCompile]
        struct CastBufferRays : IJobChunk
        {
            public BufferTypeHandle<RaycastBuffer> BufferChunk;
            [ReadOnly] public CollisionWorld world;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<RaycastBuffer> rays = chunk.GetBufferAccessor(BufferChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    DynamicBuffer<RaycastBuffer> buffer = rays[i];
                    for (int j = 0; j < buffer.Length; j++)
                    {
                        RaycastBuffer item = buffer[j];
                        world.CastRay(item.test, out item.test2);
                        buffer[j] = item;
                    }


                }
            }
        }
    }





    public struct RaycastBuffer : IBufferElementData {
        public RaycastInput test;
        public Unity.Physics.RaycastHit test2;
    }
}