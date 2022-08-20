using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using Unity.Physics;
using Unity.Jobs;
using Unity.Physics.Systems;
using UnityEngine;
using Global.Component;
using Unity.Mathematics;
using Stats;
using DreamersInc.InflunceMapSystem;
using PixelCrushers.LoveHate;
using RaycastHiy = Unity.Physics.RaycastHit;
namespace AISenses.VisionSystems
{

    public class VisionTargetingUpdateGroup : ComponentSystemGroup
    {
        public VisionTargetingUpdateGroup()
        {
            RateManager = new RateUtils.VariableRateManager(1000, true);
        }

    }

    [UpdateInGroup(typeof(VisionTargetingUpdateGroup))]
    public partial class VisionSystemJobs : SystemBase
    {
        private EntityQuery SeerEntityQuery;

        private EntityQuery TargetEntityQuery;
        EntityCommandBufferSystem entityCommandBufferSystem;

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
            entityCommandBufferSystem = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();



        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            this.RegisterPhysicsRuntimeSystemReadWrite();
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            CollisionWorld collisionWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;

            systemDeps = new VisionRayCastJob()
            {
                ScanBufferChunk = GetBufferTypeHandle<ScanPositionBuffer>(false),
                TransformChunk = GetComponentTypeHandle<LocalToWorld>(true),
                VisionChunk = GetComponentTypeHandle<Vision>(true),
                physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld,
                world = collisionWorld,
                TargetArray = TargetEntityQuery.ToComponentDataArray<AITarget>(Allocator.TempJob),
                TargetPosition = TargetEntityQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
                TargetEntity = TargetEntityQuery.ToEntityArray(Allocator.TempJob),
            }.ScheduleSingle(SeerEntityQuery, systemDeps);
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps.Complete();
            Dependency = systemDeps;

        }

        [BurstCompile]
        struct VisionRayCastJob : IJobChunk
        {
            public BufferTypeHandle<ScanPositionBuffer> ScanBufferChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> TransformChunk;
            [ReadOnly] public ComponentTypeHandle<Vision> VisionChunk;
            [DeallocateOnJobCompletion] public NativeArray<AITarget> TargetArray;
            [DeallocateOnJobCompletion] public NativeArray<LocalToWorld> TargetPosition;
            [DeallocateOnJobCompletion] public NativeArray<Entity> TargetEntity;

            [ReadOnly] public CollisionWorld world;
            [ReadOnly] public PhysicsWorld physicsWorld;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<LocalToWorld> Transforms = chunk.GetNativeArray(TransformChunk);
                NativeArray<Vision> Visions = chunk.GetNativeArray(VisionChunk);
                BufferAccessor<ScanPositionBuffer> bufferAccessor = chunk.GetBufferAccessor(ScanBufferChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    DynamicBuffer<ScanPositionBuffer> buffer = bufferAccessor[i];
                    buffer.Clear();

                    LocalToWorld transform = Transforms[i];
                    Vision vision = Visions[i];
                    for (int j = 0; j < TargetArray.Length; j++)
                    {
                        float dist = Vector3.Distance(transform.Position, TargetPosition[j].Position);
                        if (dist < vision.viewRadius)
                        {
                            Vector3 dirToTarget = ((Vector3)TargetPosition[j].Position - (Vector3)(transform.Position + new float3(0, 1, 0))).normalized;
                            if (Vector3.Angle(transform.Forward, dirToTarget) < vision.ViewAngle / 2.0f)
                            {
                                RaycastInput raycastInput = new RaycastInput()
                                {
                                    Start = transform.Position + new float3(0, 1, 0),
                                    End = TargetPosition[j].Position + TargetArray[j].CenterOffset,
                                    Filter = new CollisionFilter()
                                    {
                                        BelongsTo = (1 << 10),
                                        CollidesWith = ((1 << 10) | (1 << 11) | (1 << 12)),
                                        GroupIndex = 0
                                    }
                                };
                                if (world.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
                                {
                                    if (raycastHit.Entity.Equals(TargetEntity[j]))
                                    {
                                        buffer.Add(new ScanPositionBuffer()
                                        {
                                            target = new Target()
                                            {
                                                CanSee = true,
                                                DistanceTo = dist,
                                                LastKnownPosition = TargetPosition[j].Position,
                                                TargetInfo = TargetArray[j],
                                                entity = TargetEntity[j]
                                            },
                                            dist = dist

                                        });
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }

    }
}