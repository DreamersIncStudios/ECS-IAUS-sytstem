using Unity.Entities;
using Unity.Burst;
using IAUS.ECS.Component;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;
namespace IAUS.ECS.Systems {
    [BurstCompile]
    public struct AddMovementState<T> : IJobChunk
        where T : unmanaged, MovementState
    {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<DistanceToConsideration> Distance;

        [ReadOnly] public EntityTypeHandle EntityChunk;
        public ComponentTypeHandle<T> MovementChunk;
        [ReadOnly]public ComponentTypeHandle<LocalToWorld> ToWorldChunk;

        public BufferTypeHandle<StateBuffer> StateBufferChunk;
        [ReadOnly] public BufferTypeHandle<TravelWaypointBuffer> PatrolBufferChunk;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            NativeArray<T> MovementStyle = chunk.GetNativeArray(MovementChunk);
            NativeArray<LocalToWorld> toWorld = chunk.GetNativeArray(ToWorldChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(StateBufferChunk);
            BufferAccessor<TravelWaypointBuffer> PatrolBufferAccessor = chunk.GetBufferAccessor(PatrolBufferChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entities[i];
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
                c1.NumberOfWayPoints = buffer.Length;
                c1.CurWaypoint = buffer[0].WayPoint;
                c1.StartingDistance = Vector3.Distance(buffer[0].WayPoint.Position, toWorld[i].Position)+15;
                c1.Status = ActionStatus.Idle;
                if (add)
                {
                    stateBuffer.Add(new StateBuffer()
                    {
                        StateName = c1.name,
                        Status = ActionStatus.Idle
                    });

                    if (!Distance.HasComponent(entity))
                    {
                        entityCommandBuffer.AddComponent<DistanceToConsideration>(chunkIndex, entity);
                    }

                }
                MovementStyle[i] = c1;
            }
        }
    }

}