
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


            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;


            systemDeps = new RaycastTargets()
            {
                SeersChunk = GetArchetypeChunkComponentType<Vision>(true),
                SeersPositionChunk = GetArchetypeChunkComponentType<LocalToWorld>(true),
                ScanBufferChunk = GetArchetypeChunkBufferType<ScanPositionBuffer>(false),
                PossibleTargetPosition = TargetEntityQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
                physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld,
        }.ScheduleParallel(SeerEntityQuery, systemDeps);
            systemDeps.Complete();
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new FindClosestTarget()
            {
                DT = Time.DeltaTime,
                SeersChunk = GetArchetypeChunkComponentType<Vision>(false),
                ScanBufferChunk = GetArchetypeChunkBufferType<ScanPositionBuffer>(false),
                AItargetFromEntity = GetComponentDataFromEntity<AITarget>(false)

            }.ScheduleParallel(SeerEntityQuery, systemDeps);

            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);



            Dependency = systemDeps;





        }

        [BurstCompile]
        public struct RaycastTargets : IJobChunk
        {
            [ReadOnly]public ArchetypeChunkComponentType<Vision> SeersChunk;
            [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> SeersPositionChunk;
            public ArchetypeChunkBufferType<ScanPositionBuffer> ScanBufferChunk;
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
                    if (!seer.LookForTargets) {
                        continue;
                    }
                    
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
                                    //Debug.Log("hit Something");
                                    scan.target.entity = physicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
                                    scan.target.DistanceTo = raycastHit.Fraction*dist;
                                    scan.target.PositionSawAt = raycastHit.Position;
                                    scan.target.angleTo = Vector3.Angle(seerPosition.Forward, ((Vector3)raycastHit.Position - (Vector3)seerPosition.Position).normalized);
                                    //
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

      //  [BurstCompile]
        public struct FindClosestTarget : IJobChunk
        {
            public ArchetypeChunkComponentType<Vision> SeersChunk;
            public ArchetypeChunkBufferType<ScanPositionBuffer> ScanBufferChunk;
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
                    if (!seer.LookForTargets)
                    {
                        seer.Scantimer -= DT;
                        Seers[i] = seer;
                        continue;
                    }
                    if (buffer.Length == 0)
                        continue;

                    ScanPositionBuffer ClosestTarget = buffer[0];
                        for (int j = 0; j < buffer.Length; j++)
                        {
                        AITarget check = AItargetFromEntity[buffer[j].target.entity];
                            if (ClosestTarget.target.DistanceTo > buffer[j].target.DistanceTo && check.CanBeTargeted)
                            {
                                ClosestTarget = buffer[j];
                            }
                        }
                        seer.ClosestTarget = ClosestTarget;
 
                    seer.Scantimer = 3.5f;
                        Seers[i] = seer;
                        
                }
            }
        }
    }
}
