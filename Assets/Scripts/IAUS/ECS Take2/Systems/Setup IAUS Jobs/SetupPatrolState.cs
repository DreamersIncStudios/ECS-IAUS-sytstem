using Unity.Entities;
using Unity.Collections;
using InfluenceMap;
using Unity.Burst;

namespace IAUS.ECS2.IAUSSetup
{
    [BurstCompile]
    public struct SetupPatrolState : IJobChunk
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;

        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<DistanceToConsideration> Distance;

        [NativeDisableParallelForRestriction]
        [ReadOnly] public ComponentDataFromEntity<InfluenceValues> Influences;

        [ReadOnly]public ArchetypeChunkEntityType EntityChunk;
        public ArchetypeChunkComponentType<Patrol> PatrolChunk;
        public ArchetypeChunkBufferType<StateBuffer> StateBufferChunk;
        [ReadOnly] public ArchetypeChunkBufferType<PatrolBuffer> PatrolBufferChunk;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            NativeArray<Patrol> Patrols = chunk.GetNativeArray(PatrolChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(StateBufferChunk);
            BufferAccessor<PatrolBuffer> PatrolBufferAccessor = chunk.GetBufferAccessor(PatrolBufferChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entities[i];
                Patrol c1 = Patrols[i];
                DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];
                DynamicBuffer<PatrolBuffer> buffer = PatrolBufferAccessor[i];

                bool add = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.Patrol)
                    { add = false; }
                }
                c1.MaxNumWayPoint = buffer.Length;

                if (add)
                {
                    stateBuffer.Add(new StateBuffer()
                    {
                        StateName = AIStates.Patrol,
                        Status = ActionStatus.Idle
                    });

                    if (!Distance.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent<DistanceToConsideration>(chunkIndex, entity);
                    }
                    if (!Influences.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent<InfluenceValues>(chunkIndex, entity);
                    }
                }
                Patrols[i] = c1;
            }
        }
    }

}