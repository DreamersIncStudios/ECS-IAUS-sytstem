using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
namespace IAUS.ECS2.Component
{
    [System.Serializable]
    public struct AITarget : IComponentData
    {
        public TargetType Type;
        public float3 Position;
    }

    public enum TargetType { 
        None,Character, Location, Vehicle 
    }
    public class AITargetPositionUpdate : SystemBase {
        private EntityQuery UpdateQuery;
        protected override void OnCreate()
        {
            UpdateQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AITarget)), ComponentType.ReadOnly(typeof(LocalToWorld)) }
            });
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            Dependency = new UpdatePosition()
            {
                AITargetChunk = GetArchetypeChunkComponentType<AITarget>(false),
                PositionsChunk = GetArchetypeChunkComponentType<LocalToWorld>(true)
            }.ScheduleParallel(UpdateQuery, Dependency);
        }

        [Unity.Burst.BurstCompile]
        public struct UpdatePosition : IJobChunk
        {
            public ArchetypeChunkComponentType<AITarget> AITargetChunk;
            [ReadOnly]public ArchetypeChunkComponentType<LocalToWorld> PositionsChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<AITarget> Targets = chunk.GetNativeArray(AITargetChunk);
                NativeArray<LocalToWorld> Positions = chunk.GetNativeArray(PositionsChunk);
                
                for (int i = 0; i < chunk.Count; i++)
                {
                    AITarget target = Targets[i];
                    target.Position = Positions[i].Position;
                    Targets[i] = target;
                }

            }
        }
    }

}