using Unity.Entities;
using Unity.Burst;
using IAUS.ECS.Component;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst.Intrinsics;

namespace IAUS.ECS.Systems {
    [BurstCompile]
    public struct AddMovementState<T> : IJobChunk
        where T : unmanaged, MovementState
    {

        public ComponentTypeHandle<T> MovementChunk;
        [ReadOnly]public ComponentTypeHandle<LocalTransform> ToWorldChunk;

        public BufferTypeHandle<StateBuffer> StateBufferChunk;
        [ReadOnly] public BufferTypeHandle<TravelWaypointBuffer> PatrolBufferChunk;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<T> MovementStyle = chunk.GetNativeArray(ref MovementChunk);
            NativeArray<LocalTransform> toWorld = chunk.GetNativeArray( ref ToWorldChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor( ref StateBufferChunk);
            BufferAccessor<TravelWaypointBuffer> PatrolBufferAccessor = chunk.GetBufferAccessor(ref PatrolBufferChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                T c1 = MovementStyle[i];
                DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];
                DynamicBuffer<TravelWaypointBuffer> buffer = PatrolBufferAccessor[i];

                bool add = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.Patrol)
                    {
                        add = false;
                        continue;
                    }
                }
                c1.NumberOfWayPoints = (uint)buffer.Length;
                c1.CurWaypoint = buffer[0].WayPoint;
                c1.StartingDistance = Vector3.Distance(buffer[0].WayPoint.Position, toWorld[i].Position)+15;
                c1.Status = ActionStatus.Idle;
                if (add)
                {
                    stateBuffer.Add(new StateBuffer(c1.name));
                }
                MovementStyle[i] = c1;
            }
        }

     
    }

}