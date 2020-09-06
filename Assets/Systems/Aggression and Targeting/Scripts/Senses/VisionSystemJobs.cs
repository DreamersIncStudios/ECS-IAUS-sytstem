using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

namespace CharacterAlignmentSystem.SensesJob
{
    public class VisionSystem : SystemBase
    {
        private EntityQuery SeerEntityQuery;
        private EntityQuery TargetEntityQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            SeerEntityQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Vision)), ComponentType.ReadOnly(typeof(LocalToWorld)), ComponentType.ReadWrite(typeof(ScanPositionBuffer)) }
            });

            TargetEntityQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(LocalToWorld))},
                Any = new ComponentType[] { ComponentType.ReadOnly(typeof(Angel)),ComponentType.ReadOnly(typeof(Human)),ComponentType.ReadOnly(typeof(Daemon)),
                    ComponentType.ReadOnly(typeof(Mecha)) }
            }); 
        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;


            systemDeps = new CreateBufferOfTargetsToRaycast() {
                SeersChunk = GetArchetypeChunkComponentType<Vision>(false),
                ScanBuffer = GetArchetypeChunkBufferType<ScanPositionBuffer>(false),
                SeersPositionChunk = GetArchetypeChunkComponentType<LocalToWorld>(true),
                PossibleTargetPosition = TargetEntityQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob)
            }.ScheduleParallel(SeerEntityQuery, systemDeps);

            Dependency = systemDeps;

        }
    }


    [BurstCompile]
    public struct CreateBufferOfTargetsToRaycast : IJobChunk
    {
        public ArchetypeChunkComponentType<Vision> SeersChunk;
        public ArchetypeChunkBufferType<ScanPositionBuffer> ScanBuffer;
        [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> SeersPositionChunk;
        [ReadOnly][DeallocateOnJobCompletion]public NativeArray<LocalToWorld> PossibleTargetPosition;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Vision> Seers = chunk.GetNativeArray(SeersChunk);
            NativeArray<LocalToWorld> SeersPosition = chunk.GetNativeArray(SeersPositionChunk);
            BufferAccessor<ScanPositionBuffer> Buffers = chunk.GetBufferAccessor(ScanBuffer);
            for (int i = 0; i < chunk.Count; i++)
            {
                Vision seer = Seers[i];
                LocalToWorld seerPosition = SeersPosition[i];
                DynamicBuffer<ScanPositionBuffer> buffer = Buffers[i];
                buffer.Clear();
                for (int j = 0; j < PossibleTargetPosition.Length; j++)
                {
                    float dist = Vector3.Distance(seerPosition.Position, PossibleTargetPosition[j].Position);
                    if (dist <= seer.viewRadius && dist !=0.0f) 
                    {
                        Vector3 dirToTarget = ((Vector3)PossibleTargetPosition[j].Position - (Vector3)seerPosition.Position).normalized;
                        if (Vector3.Angle(seerPosition.Forward, dirToTarget) < seer.ViewAngle / 2.0f) 
                        {
                            buffer.Add(new ScanPositionBuffer()
                            {
                                target = new Target()
                                { position = PossibleTargetPosition[j].Position }
                            });
                        }
                    }
                }

                Seers[i] = seer;
               

            }

        }
    }


}