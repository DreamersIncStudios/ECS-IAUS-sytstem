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
    public class VisionSystemJobs : SystemBase
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
        float interval = 1.0f;
        bool runUpdate => interval <= 0.0f;
        protected override void OnUpdate()
        {
            if (runUpdate)
            {
                CollisionWorld collisionWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;
                Dependency = JobHandle.CombineDependencies(Dependency, m_EndFramePhysicsSystem.GetOutputDependency());
                JobHandle systemDeps = Dependency;
                systemDeps = new AddRaycastBuffer()
                {
                    BufferChunk = GetBufferTypeHandle<ScanPositionBuffer>(false),
                    transformChunk = GetComponentTypeHandle<LocalToWorld>(true),
                    VisionChunk = GetComponentTypeHandle<Vision>(true),
                    PossibleTargetPosition = TargetEntityQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob)

                }.ScheduleParallel(SeerEntityQuery, systemDeps);
                entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

                systemDeps = new CastBufferRays()
                {
                    world = collisionWorld,
                    BufferChunk = GetBufferTypeHandle<ScanPositionBuffer>(false),
                    TargetInfo = GetComponentDataFromEntity<AITarget>(false),
                    physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld

                }.ScheduleParallel(SeerEntityQuery, systemDeps);

                entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
                World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().AddInputDependency(systemDeps);
                Dependency = systemDeps;
                interval = 1.0f;
            }
            else
            {
                interval -= 1 / 60.0f;
            }
        }

        [BurstCompile]
        struct AddRaycastBuffer : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> transformChunk;
            [ReadOnly] public ComponentTypeHandle<Vision> VisionChunk;
            [NativeDisableParallelForRestriction] [DeallocateOnJobCompletion] public NativeArray<LocalToWorld> PossibleTargetPosition;
            public BufferTypeHandle<ScanPositionBuffer> BufferChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Vision> vision = chunk.GetNativeArray(VisionChunk);
                NativeArray<LocalToWorld> tranforms = chunk.GetNativeArray(transformChunk);
                BufferAccessor<ScanPositionBuffer> bufferAccessor = chunk.GetBufferAccessor(BufferChunk);
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
                                bufferAccessor[i].Add(new ScanPositionBuffer()
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
                                    },
                                    dist = dist
                                }); ;

                            }
                        }
                    }

                }
            }
        }

        [BurstCompile]
        struct CastBufferRays : IJobChunk
        {
            public BufferTypeHandle<ScanPositionBuffer> BufferChunk;
            [ReadOnly] [NativeDisableParallelForRestriction] public ComponentDataFromEntity<AITarget> TargetInfo;
            [ReadOnly] public CollisionWorld world;
            [ReadOnly] public PhysicsWorld physicsWorld;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<ScanPositionBuffer> rays = chunk.GetBufferAccessor(BufferChunk);

                for (int i = 0; i < chunk.Count; i++)
                {

                    DynamicBuffer<ScanPositionBuffer> buffer = rays[i];
                    for (int j = 0; j < buffer.Length; j++)
                    {
                        ScanPositionBuffer item = buffer[j];
                        if (world.CastRay(item.test, out Unity.Physics.RaycastHit raycastHit))
                        {
                            if (TargetInfo.HasComponent(raycastHit.Entity))
                            {
                                Target setTarget = new Target();
                                setTarget.entity = physicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
                                setTarget.TargetInfo = TargetInfo[setTarget.entity];

                                setTarget.DistanceTo = raycastHit.Fraction * item.dist;
                                setTarget.LastKnownPosition = raycastHit.Position;
                                setTarget.CanSee = true;
                                item.target = setTarget;
                                buffer[j] = item;
                            }
                        }
                        else
                        {
                            buffer.RemoveAt(j);
                        }
                        //if(item.target.entity== Entity.Null)
                        //    buffer.RemoveAt(j);

                    }
                }
            }

        }

    }
}