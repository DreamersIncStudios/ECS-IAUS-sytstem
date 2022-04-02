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

namespace AISenses.VisionSystems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public class VisionSystemJobs : SystemBase
    {
        private EntityQuery SeerEntityQuery;

        private EntityQuery TargetEntityQuery;
        BuildPhysicsWorld m_BuildPhysicsWorld;
        EndFramePhysicsSystem m_EndFramePhysicsSystem;
        EndFixedStepSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem;

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
            m_BuildPhysicsWorld = World.GetExistingSystem<BuildPhysicsWorld>();
            m_EndFramePhysicsSystem = World.GetExistingSystem<EndFramePhysicsSystem>();
            m_EntityCommandBufferSystem = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();



        }
        float interval = 0.0f;
        bool runUpdate => interval <= 0.0f;
        protected override void OnUpdate()
        {
            if (runUpdate)
            {
                m_BuildPhysicsWorld = World.GetExistingSystem<BuildPhysicsWorld>();
                CollisionWorld collisionWorld = m_BuildPhysicsWorld.PhysicsWorld.CollisionWorld;
                EntityCommandBuffer commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer();
                Dependency = JobHandle.CombineDependencies(Dependency, m_EndFramePhysicsSystem.GetOutputDependency());
                JobHandle systemDeps = Dependency;
                World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().AddInputDependencyToComplete( systemDeps);

                systemDeps = new AddRaycastBuffer()
                {
                    BufferChunk = GetBufferTypeHandle<ScanPositionBuffer>(false),
                    transformChunk = GetComponentTypeHandle<LocalToWorld>(true),
                    VisionChunk = GetComponentTypeHandle<Vision>(true),
                    EntityChunk = GetEntityTypeHandle(),
                    PossibleTargetPosition = TargetEntityQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
                    PossibleTargetEntities = TargetEntityQuery.ToEntityArray(Allocator.TempJob),
                    AITargets = TargetEntityQuery.ToComponentDataArray<AITarget>(Allocator.TempJob)

                }.ScheduleParallel(SeerEntityQuery, systemDeps);
                m_EntityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

                systemDeps.Complete();
                ComponentDataFromEntity<AITarget> targert = GetComponentDataFromEntity<AITarget>(true);
                ComponentDataFromEntity<InfluenceComponent> Influence = GetComponentDataFromEntity<InfluenceComponent>(true);

                Entities.WithoutBurst().ForEach((DynamicBuffer<ScanPositionBuffer> buffer, ref EnemyStats stats, ref InfluenceComponent self) => {
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        ScanPositionBuffer item = buffer[i];
                        int index = i;
                        if (Physics.Raycast(item.castRay.Start, item.castRay.Dir, out UnityEngine.RaycastHit hit, Mathf.Infinity))
                        {
                            if (hit.rigidbody != null)
                            {
                                if (hit.rigidbody.gameObject.layer == 10 || hit.rigidbody.gameObject.layer == 11 || hit.rigidbody.gameObject.layer == 12)
                                {
                                    Entity entityHit = hit.rigidbody.gameObject.GetComponent<BaseCharacter>().SelfEntityRef;
                                    InfluenceComponent influence = Influence[entityHit];
                                   float affinity = LoveHate.factionDatabase.GetFaction(influence.factionID).GetPersonalAffinity(self.factionID);
                                    if (affinity < 50) //set to threshold
                                    {
                                        item.target.TargetInfo = targert[entityHit];
                                        item.target.entity = entityHit;
                                        item.target.DistanceTo =  item.dist;
                                        item.target.LastKnownPosition = hit.rigidbody.gameObject.transform.position;
                                        item.target.CanSee = true;
                                        buffer[i] = item;
                                    }
                                }
                            }
                        }
                    }
                    
                }).Run();
                Entities.WithoutBurst().ForEach((DynamicBuffer<ScanPositionBuffer> buffer, ref PlayerStatComponent stats) => {

                    for (int i = 0; i < buffer.Length; i++)
                    {
                        ScanPositionBuffer item = buffer[i];
                        int index = i;
                        if (Physics.Raycast(item.castRay.Start, item.castRay.Dir, out UnityEngine.RaycastHit hit))
                        {
                            if (hit.rigidbody != null)
                            {
                                if (hit.rigidbody.gameObject.layer == 11 || hit.rigidbody.gameObject.layer == 12)
                                {
                                    Entity entityHit = hit.rigidbody.gameObject.GetComponent<BaseCharacter>().SelfEntityRef;
                                    item.target.TargetInfo = targert[entityHit];
                                    item.target.entity = entityHit;
                                    buffer[i] = item;
                                }
                                else
                                    buffer.RemoveAt(index);
                            }
                            else
                                buffer.RemoveAt(index);
                        };

                        if(item.target.entity== Entity.Null)
                            buffer.RemoveAt(i);
                    }
                }).Run();

                //systemDeps = new CastBufferRays()
                //{
                //    world = collisionWorld,
                //    BufferChunk = GetBufferTypeHandle<ScanPositionBuffer>(false),
                //    TargetInfo = GetComponentDataFromEntity<AITarget>(false),
                //    physicsWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld

                //}.ScheduleParallel(SeerEntityQuery, systemDeps);

                //m_EntityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
                Dependency = systemDeps;
                interval = .50f;
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
            [ReadOnly] public EntityTypeHandle EntityChunk;
            [NativeDisableParallelForRestriction] [DeallocateOnJobCompletion] public NativeArray<LocalToWorld> PossibleTargetPosition;
            [NativeDisableParallelForRestriction][DeallocateOnJobCompletion] public NativeArray<Entity> PossibleTargetEntities;
            [NativeDisableParallelForRestriction][DeallocateOnJobCompletion] public NativeArray<AITarget> AITargets;


            public BufferTypeHandle<ScanPositionBuffer> BufferChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Vision> vision = chunk.GetNativeArray(VisionChunk);
                NativeArray<LocalToWorld> tranforms = chunk.GetNativeArray(transformChunk);
                NativeArray<Entity> Entities = chunk.GetNativeArray(EntityChunk);

                BufferAccessor<ScanPositionBuffer> bufferAccessor = chunk.GetBufferAccessor(BufferChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    bufferAccessor[i].Clear();
                    for (int j = 0; j < PossibleTargetEntities.Length; j++)
                    {
                        if (Entities[i] != PossibleTargetEntities[j])
                        {

                            float3 test = PossibleTargetPosition[j].Position + AITargets[j].CenterOffset;
                            float dist = Vector3.Distance(tranforms[i].Position, test);
                            if (dist <= vision[i].viewRadius)
                            {
                                Vector3 dirToTarget = ((Vector3)test - (Vector3)(tranforms[i].Position + new float3(0, 1, 0))).normalized;
                                if (Vector3.Angle(tranforms[i].Forward, dirToTarget) < vision[i].ViewAngle / 2.0f)
                                {


                                    bufferAccessor[i].Add(new ScanPositionBuffer()
                                    {
                                        rayToCast = new RaycastInput()
                                        {
                                            Start = tranforms[i].Position + new float3(0, 1, 0),
                                            End = test,
                                            Filter = new CollisionFilter
                                            {
                                                BelongsTo = ~0u,
                                                CollidesWith = ((1 << 10) | (1 << 11) | (1 << 12)),
                                                GroupIndex = 0
                                            }
                                        },
                                        dist = dist,
                                        castRay = new CastRay() {
                                            Start = tranforms[i].Position + new float3(0, 1, 0),
                                            Dir = dirToTarget,
                                            dist = dist,
                                            Targets = ((1 << 10) | (1 << 11) | (1 << 12))

                                        }

                                    }); 
                                }
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
                        Debug.DrawLine(item.rayToCast.Start, item.rayToCast.End, Color.red, 1); 
                        if (world.CastRay(item.rayToCast, out Unity.Physics.RaycastHit raycastHit))
                        {
                            Target setTarget = new Target();
                            setTarget.entity = raycastHit.Entity;
                            if (TargetInfo.HasComponent(raycastHit.Entity))
                            {
                
                                setTarget.TargetInfo = TargetInfo[raycastHit.Entity];

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
              

                    }

                }
            }

        }

    }
}