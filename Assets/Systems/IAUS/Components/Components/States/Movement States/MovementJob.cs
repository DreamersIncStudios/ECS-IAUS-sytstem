using Unity.Entities;
using IAUS.ECS.Component;
using Unity.Collections;
using Unity.Burst.Intrinsics;

namespace IAUS.ECS.Systems
{
    //TODO rework job
    //  [BurstCompile]
    public struct CheckThreatAtWaypoint<T> : IJobChunk
        where T : unmanaged, IMovementState

    {
        public BufferTypeHandle<TravelWaypointBuffer> PatrolBuffer;
        [ReadOnly] public ComponentTypeHandle<IAUSBrain> IAUSBrainChunk;
        public ComponentTypeHandle<Patrol> PatrolChunk;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            BufferAccessor<TravelWaypointBuffer> bufferAccess = chunk.GetBufferAccessor(ref PatrolBuffer);
            NativeArray<IAUSBrain> Brains = chunk.GetNativeArray(ref IAUSBrainChunk);
            NativeArray<Patrol> patrols = chunk.GetNativeArray(ref PatrolChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                DynamicBuffer<TravelWaypointBuffer> buffer = bufferAccess[i];
                Patrol patrol = patrols[i];

                for (int j = 0; j < buffer.Length; j++)
                {
                    TravelWaypointBuffer point = buffer[j];
                    point.WayPoint.InfluenceAtPosition = 0.0f; // todo replace with system;
                    buffer[j] = point;
                    if (j == patrol.WaypointIndex)
                    {
                        patrol.CurWaypoint = point.WayPoint;
                    }
                }

                patrols[i] = patrol;

            }
        }
    }

}
