
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using CharacterAlignmentSystem;
using Unity.Mathematics;

using Dreamers.Global;

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
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(LocalToWorld)) },
                Any = new ComponentType[] { ComponentType.ReadOnly(typeof(Angel)),ComponentType.ReadOnly(typeof(Human)),ComponentType.ReadOnly(typeof(Daemon)),
                    ComponentType.ReadOnly(typeof(Mecha)) }
            });


            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;


            systemDeps = new CreateBufferOfTargetsToRaycast() {
                SeersChunk = GetArchetypeChunkComponentType<Vision>(false),
                ScanBufferChunk = GetArchetypeChunkBufferType<ScanPositionBuffer>(false),
                SeersPositionChunk = GetArchetypeChunkComponentType<LocalToWorld>(true),
                PossibleTargetPosition = TargetEntityQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
            }.ScheduleParallel(SeerEntityQuery, systemDeps);
     Dependency = systemDeps;



       
   
        }
    }
    [UpdateAfter(typeof(VisionSystem))]
    public class TestVisionRay : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.
                ForEach(( DynamicBuffer<ScanPositionBuffer> Buffer, ref Vision vision, ref FreqPhasing Freq, ref AlertLevel alert) =>
            {
                //for (int i = 0; i < Buffer.Length; i++)
                //{
                //    Debug.DrawRay(Buffer[i].target.raycastCenter.from, Buffer[i].target.raycastCenter.direction*vision.viewRadius, Color.red);
                //    Debug.DrawRay(Buffer[i].target.raycastCenterLeft.from, Buffer[i].target.raycastCenterLeft.direction* vision.viewRadius, Color.red);
                //    Debug.DrawRay(Buffer[i].target.raycastCenterRight.from, Buffer[i].target.raycastCenterRight .direction* vision.viewRadius, Color.red);
                //}
                if (UnityEngine.Time.frameCount % vision.DetectionRate == Freq.Phasing)
                {
                    for (int i = 0; i < Buffer.Length; i++)
                    {
                        ScanPositionBuffer RayScan = Buffer[i];
                    
                        NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(3, Allocator.TempJob);
                        NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(3, Allocator.TempJob);
                        commands[0] = RayScan.target.raycastCenter;



                        commands[1] = RayScan.target.raycastCenterLeft;
                        commands[2] = RayScan.target.raycastCenterRight;
                        // Schedule the batch of raycasts
                        JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 3);

                        // Wait for the batch processing job to complete
                        handle.Complete();

                        if (Hit(results[0]))
                        {
                            RayScan.target.HitCount += 2;
                            Debug.Log("hit");
                        }
                        if (Hit(results[1]))
                            RayScan.target.HitCount += 1;
                        if (Hit(results[2]))
                            RayScan.target.HitCount += 1;

                        commands.Dispose();
                        results.Dispose();

                    }
                }
            });
        }


        public bool Hit(RaycastHit Result)
        {
            if (Result.collider != null)
            {
                return Result.collider.gameObject.layer == 11 ||
                    Result.collider.gameObject.layer == 12 ||
                    Result.collider.gameObject.layer == 10;

            }
            return false;
        }
    }


    [BurstCompile]
    public struct CreateBufferOfTargetsToRaycast : IJobChunk
    {
        public ArchetypeChunkComponentType<Vision> SeersChunk;
        public ArchetypeChunkBufferType<ScanPositionBuffer> ScanBufferChunk;
        [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> SeersPositionChunk;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<LocalToWorld> PossibleTargetPosition;


        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Vision> Seers = chunk.GetNativeArray(SeersChunk);
            NativeArray<LocalToWorld> SeersPosition = chunk.GetNativeArray(SeersPositionChunk);
            BufferAccessor<ScanPositionBuffer> Buffers = chunk.GetBufferAccessor(ScanBufferChunk);
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

                            buffer.Add(new ScanPositionBuffer()
                            {

                                target = new Target()
                                {
                                    raycastCenter = new RaycastCommand()
                                    {
                                        from = seerPosition.Position,
                                        distance = dist,
                                        direction = ((Vector3)PossibleTargetPosition[j].Position - (Vector3)seerPosition.Position).normalized,
                                        maxHits = 1,
                                        layerMask = ~seer.ObstacleMask
                                    },
                                    raycastCenterLeft = new RaycastCommand()
                                    {
                                        from = seerPosition.Position,
                                        distance = dist,
                                        direction = ((Vector3)(PossibleTargetPosition[j].Position - new float3(-2, 0, 0)) - (Vector3)seerPosition.Position).normalized,
                                        maxHits = 1,
                                        layerMask = ~seer.ObstacleMask
                                    },
                                    raycastCenterRight = new RaycastCommand()
                                    {
                                        from = seerPosition.Position,
                                        distance = dist,
                                        direction = ((Vector3)(PossibleTargetPosition[j].Position - new float3(2, 0, 0)) - (Vector3)seerPosition.Position).normalized,
                                        maxHits = 1,
                                        layerMask = ~seer.ObstacleMask
                                    }
                                }
                            });
                        }

                    }
                }

                Seers[i] = seer;

            }

        }
    }
   
 

}