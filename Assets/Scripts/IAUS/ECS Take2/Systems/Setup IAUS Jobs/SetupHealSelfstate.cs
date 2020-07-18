
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using InfluenceMap;
namespace IAUS.ECS2.IAUSSetup
{
    [BurstCompile]

    public struct SetupHealSelfState : IJobChunk
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<HealthConsideration> health;
        [ReadOnly]
        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<InfluenceValues> Influences;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            throw new System.NotImplementedException();
        }
    }
}
