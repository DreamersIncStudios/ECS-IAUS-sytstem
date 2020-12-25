
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
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(LocalToWorld)), ComponentType.ReadOnly(typeof(AITarget)) }

            });


            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;


            systemDeps = new RaycastTargets()
            {
                SeersChunk = GetArchetypeChunkComponentType<Vision>(false),
                SeersPositionChunk = GetArchetypeChunkComponentType<LocalToWorld>(true),
                PossibleTargetPosition = TargetEntityQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
                physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld
        }.ScheduleParallel(SeerEntityQuery, systemDeps);
            systemDeps.Complete();
            Dependency = systemDeps;





        }

        [BurstCompile]
        public struct RaycastTargets : IJobChunk
        {
            public ArchetypeChunkComponentType<Vision> SeersChunk;
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
                    LocalToWorld seerPosition = SeersPosition[i];
                    DynamicBuffer<ScanPositionBuffer> buffer = Buffers[i];
                    buffer.Clear();
                    for (int j = 0; j < PossibleTargetPosition.Length; j++)
                    {
                        float dist = Vector3.Distance(seerPosition.Position, PossibleTargetPosition[j].Position);
                        if (dist <= seer.viewRadius && dist != 0.0f)
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
                                        CollidesWith = ~10U,
                                        GroupIndex = 0
                                    }
                                };
                                RaycastInput raycastCenterLeft = new RaycastInput()
                                {
                                    Start = seerPosition.Position,
                                    End = ((Vector3)(PossibleTargetPosition[j].Position - new float3(-2, 0, 0))),
                                    Filter = new CollisionFilter
                                    {
                                        BelongsTo = ~0u,
                                        CollidesWith = ~10U,
                                        GroupIndex = 0
                                    }
                                };
                                RaycastInput raycastCenterRight = new RaycastInput
                                {
                                    Start = seerPosition.Position,
                                    End = ((Vector3)(PossibleTargetPosition[j].Position - new float3(2, 0, 0))),
                                    Filter = new CollisionFilter
                                    {
                                        BelongsTo = ~0u,
                                        CollidesWith = ~10U,
                                        GroupIndex = 0
                                    }
                                };

                                // cast rays
                        

                                if (collisionWorld.CastRay(raycastCenter, out  Unity.Physics.RaycastHit raycastHit)) {
                                    //hit something
                                  //  Debug.Log("hit Something");
                                }
                                if (collisionWorld.CastRay(raycastCenterRight, out Unity.Physics.RaycastHit raycastHit1))
                                {
                                    //hit something
                                    //Debug.Log("hit Something");

                                }
                                if (collisionWorld.CastRay(raycastCenterLeft, out Unity.Physics.RaycastHit  raycastHit2))
                                {
                                    //hit something
                                    //Debug.Log("hit Something");

                                }

                            }
                        }

                    }
                    

                    Seers[i] = seer;

                }

            }
        }


    }


}




    
   
 

