using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Global.Component;
using Unity.Physics;
using Unity.Physics.Systems;

namespace AISenses.VisionSystems
{
    public class VisionSystem : SystemBase
    {
        private EntityQuery SeerEntityQuery;
        private EntityQuery SeerEntityQueryWoPlayer;

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

            SeerEntityQueryWoPlayer = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Vision)), ComponentType.ReadOnly(typeof(LocalToWorld)), ComponentType.ReadWrite(typeof(ScanPositionBuffer)) },
                None = new ComponentType[] {ComponentType.ReadOnly(typeof(PlayerParty)) }

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
            if (UnityEngine.Time.frameCount % 240 == 10)
            {
                Dependency = JobHandle.CombineDependencies(Dependency, m_EndFramePhysicsSystem.GetOutputDependency());
                JobHandle systemDeps = Dependency;

                systemDeps = new RaycastTargets()
                {
                    SeersChunk = GetComponentTypeHandle<Vision>(true),
                    SeersPositionChunk = GetComponentTypeHandle<LocalToWorld>(true),
                    ScanBufferChunk = GetBufferTypeHandle<ScanPositionBuffer>(false),
                    PossibleTargetPosition = TargetEntityQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
                    physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld,
                }.ScheduleSingle(SeerEntityQuery, systemDeps);
                entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
                World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().AddInputDependency(systemDeps);
                systemDeps.Complete();

                systemDeps = new FindClosestTarget()
                {
                    DT = Time.DeltaTime,
                    SeersChunk =GetComponentTypeHandle<Vision>(false),
                    ScanBufferChunk = GetBufferTypeHandle<ScanPositionBuffer>(false),
                    AItargetFromEntity = GetComponentDataFromEntity<AITarget>(false)

                }.ScheduleParallel(SeerEntityQuery, systemDeps);

                entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
                Dependency = systemDeps;

            }
        }

        [BurstCompile]
        public struct RaycastTargets : IJobChunk
        {
            [ReadOnly]public ComponentTypeHandle<Vision> SeersChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> SeersPositionChunk;
            public BufferTypeHandle<ScanPositionBuffer> ScanBufferChunk;
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<LocalToWorld> PossibleTargetPosition;
            public PhysicsWorld physicsWorld;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Vision> Seers = chunk.GetNativeArray(SeersChunk);
                NativeArray<LocalToWorld> SeersPosition = chunk.GetNativeArray(SeersPositionChunk);
                BufferAccessor<ScanPositionBuffer> Buffers = chunk.GetBufferAccessor(ScanBufferChunk);

                CollisionWorld collisionWorld = physicsWorld.CollisionWorld;

                
                for (int i = 0; i < chunk.Count; i++)
                {
                    Vision seer = Seers[i];
                    
                    LocalToWorld seerPosition = SeersPosition[i];
                    DynamicBuffer<ScanPositionBuffer> buffer = Buffers[i];
                    buffer.Clear();
                    for (int j = 0; j < PossibleTargetPosition.Length; j++)
                    {
                        float dist = Vector3.Distance(seerPosition.Position, PossibleTargetPosition[j].Position);
                        if (dist <= seer.viewRadius)
                        {
                            Vector3 dirToTarget = ((Vector3)PossibleTargetPosition[j].Position - (Vector3)seerPosition.Position).normalized;
                            if (Vector3.Angle(seerPosition.Forward, dirToTarget) < seer.ViewAngle / 2.0f)
                            {
                                //Make rays 
                                RaycastInput raycastCenter = new RaycastInput()
                                {
                                    Start = seerPosition.Position,
                                    End = PossibleTargetPosition[j].Position,
                                    Filter = new CollisionFilter
                                    {
                                        BelongsTo = ~0u,
                                        CollidesWith = ((1 << 10) | (1 << 11) | (1 << 12)),
                                        GroupIndex = 0
                                    }
                                };

                                // cast rays
                                ScanPositionBuffer scan = new ScanPositionBuffer();

                                if (collisionWorld.CastRay(raycastCenter, out Unity.Physics.RaycastHit raycastHit))
                                {
                                    scan.target.entity = physicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
                                    scan.target.DistanceTo = raycastHit.Fraction*dist;
                                    scan.target.LastKnownPosition = raycastHit.Position;
                                    scan.target.CanSee = true;
                                    if(!ChecktoSeeIfwehitthisEntityBefore(buffer,scan))
                                        buffer.Add(scan);
                                }
                            }
                        }

                    }
                }
            }

            bool ChecktoSeeIfwehitthisEntityBefore(DynamicBuffer<ScanPositionBuffer> Input, ScanPositionBuffer Check) { 
                bool result =false;
                for (int i = 0; i < Input.Length; i++)
                {
                    if (Input[i].target.entity.Equals(Check.target.entity))
                    {
                        result = true;
                        return result;
                    }
                }
                return result;
            }
        }

        [BurstCompile]
        public struct FindClosestTarget : IJobChunk
        {
            public ComponentTypeHandle<Vision> SeersChunk;
            public BufferTypeHandle<ScanPositionBuffer> ScanBufferChunk;
           [NativeDisableParallelForRestriction] public ComponentDataFromEntity<AITarget> AItargetFromEntity;
            public float DT;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Vision> Seers = chunk.GetNativeArray(SeersChunk);
                BufferAccessor<ScanPositionBuffer> Buffers = chunk.GetBufferAccessor(ScanBufferChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    DynamicBuffer<ScanPositionBuffer> buffer = Buffers[i];
             

                    Vision seer = Seers[i];

                    if (buffer.Length == 0)
                        continue;
                    NativeArray<ScanPositionBuffer> scans = buffer.AsNativeArray();
                    scans.Sort(new TargetDistanceComprar());
                        seer.ClosestTarget = buffer[0];
 
                    seer.Scantimer = 3.5f;
                        Seers[i] = seer;
                }
            }

            public struct TargetDistanceComprar : System.Collections.Generic.IComparer<ScanPositionBuffer>
            {
                public int Compare(ScanPositionBuffer LHS, ScanPositionBuffer RHS)
                {
                    return LHS.target.DistanceTo.CompareTo(RHS.target.DistanceTo);
                }
            
            }
        }
    }
}
