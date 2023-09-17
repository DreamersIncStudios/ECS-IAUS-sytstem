using Unity.Entities;
using Unity.Burst;
using IAUS.ECS.Component;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst.Intrinsics;
using System.Runtime.InteropServices;
using Utilities;
using JetBrains.Annotations;

namespace IAUS.ECS.Systems {
    [BurstCompile]
    public struct AddMovementState<T> : IJobChunk
        where T : unmanaged, IMovementState
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
                    stateBuffer.Add(new StateBuffer(c1.Name));
                }
                MovementStyle[i] = c1;
            }
        }

     
    }
    public partial struct AddWanderState : IJobEntity {

        public EntityCommandBuffer.ParallelWriter ECB;
        public void Execute(Entity entity, [ChunkIndexInQuery]int sortkey ,ref WanderQuadrant wander, ref DynamicBuffer<StateBuffer> stateBuffer, ref LocalTransform transform)
        {
           
            bool add = true;
            for (int index = 0; index < stateBuffer.Length; index++)
            {
                if (stateBuffer[index].StateName == AIStates.WanderQuadrant)
                {
                    add = false;
                    continue;
                }
            }
            wander.SpawnPosition = transform.Position;

            wander.Status = ActionStatus.Idle;
           
            if (add)
            {
                stateBuffer.Add(new StateBuffer(wander.Name));
                ECB.AddComponent(sortkey, entity, new UpdateWanderLocationTag());
            }

        }

        
    }
    public struct UpdateWanderLocationTag : IComponentData { }
}