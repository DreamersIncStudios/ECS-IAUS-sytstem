using Unity.Entities;
using Unity.Burst;
using IAUS.ECS2.Component;
using Unity.Collections;
namespace IAUS.ECS2.Systems {
    [BurstCompile]
    public struct AddPatrolState : IJobChunk
    {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<DistanceToConsideration> Distance;

        [ReadOnly] public EntityTypeHandle EntityChunk;
        public ComponentTypeHandle<Patrol> PatrolChunk;
        public BufferTypeHandle<StateBuffer> StateBufferChunk;
        [ReadOnly] public BufferTypeHandle<PatrolWaypointBuffer> PatrolBufferChunk;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            NativeArray<Patrol> Patrols = chunk.GetNativeArray(PatrolChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(StateBufferChunk);
            BufferAccessor<PatrolWaypointBuffer> PatrolBufferAccessor = chunk.GetBufferAccessor(PatrolBufferChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entities[i];
                Patrol c1 = Patrols[i];
                DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];
                DynamicBuffer<PatrolWaypointBuffer> buffer = PatrolBufferAccessor[i];

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
                c1.Status = ActionStatus.Idle;
                if (add)
                {
                    stateBuffer.Add(new StateBuffer()
                    {
                        StateName = AIStates.Patrol,
                        Status = ActionStatus.Idle
                    });

                    if (!Distance.HasComponent(entity))
                    {
                        entityCommandBuffer.AddComponent<DistanceToConsideration>(chunkIndex, entity);
                    }

                }
                Patrols[i] = c1;
            }
        }
    }

}