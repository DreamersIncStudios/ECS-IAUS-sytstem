using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;

namespace CharacterAlignmentSystem.SensesJob
{
    [BurstCompile]
    public struct CreateBufferOfTargetsToRaycast : IJobChunk
    {
        public ArchetypeChunkComponentType<Vision> SeersChunk;
        public ArchetypeChunkBufferType<ScanPositionBuffer> ScanBuffer;
        [ReadOnly]public ArchetypeChunkComponentType<LocalToWorld> SeersPositionChunk;
        [ReadOnly]public NativeArray<LocalToWorld> PossibleTargetPosition;

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
                for (int j = 0; j < PossibleTargetPosition.Length; j++)
                {
                    float dist = Vector3.Distance(seerPosition.Position, PossibleTargetPosition[j].Position);
                    if (dist <= seer.viewRadius) 
                    {
                        Vector3 dirToTarget = ((Vector3)PossibleTargetPosition[j].Position - (Vector3)seerPosition.Position).normalized;
                        if (Vector3.Angle(seerPosition.Forward, dirToTarget) < seer.viewAngleXZ / 2.0f) 
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