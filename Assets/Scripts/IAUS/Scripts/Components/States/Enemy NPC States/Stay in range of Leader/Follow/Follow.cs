using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
namespace IAUS.ECS2.Component
{

    public class Follow : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Leader LeaderEntity;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            FollowEntityTag data = new FollowEntityTag() {
                Leader = LeaderEntity.self
        };
            dstManager.AddComponentData(entity, data);
            dstManager.AddComponent<SetupUpLeaderTag>(entity);
        }

    }
    public struct SetupUpLeaderTag : IComponentData { }
    public struct FollowEntityTag : IComponentData
    {
        public Entity Leader;

    }

    public class FollowSetupSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            buffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer();
        }
        EntityCommandBuffer buffer;

        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, Follow call, ref FollowEntityTag tag, ref SetupUpLeaderTag UP) => 
            {
                tag.Leader = call.LeaderEntity.self;
                buffer.RemoveComponent<SetupUpLeaderTag>(entity);
            });
        }
    }

    public class FollowUpdateSystem : SystemBase {
        private EntityQuery Followers;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            Followers = GetEntityQuery(new EntityQueryDesc()
            { 
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(StayInRange)) ,ComponentType.ReadOnly(typeof(FollowEntityTag)),ComponentType.ReadOnly(typeof(LocalToWorld))}
            });
        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            systemDeps = new InRange() {
                StayChunk = GetArchetypeChunkComponentType<StayInRange>(false),
                FollowerChunk = GetArchetypeChunkComponentType<FollowEntityTag>(true),
                PositionChunk = GetArchetypeChunkComponentType<LocalToWorld>(true),
                EntityPositions = GetComponentDataFromEntity<LocalToWorld>()
            }.ScheduleParallel(Followers,systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }

        struct InRange : IJobChunk
        {
            public ArchetypeChunkComponentType<StayInRange> StayChunk;
           [ReadOnly] public ArchetypeChunkComponentType<FollowEntityTag> FollowerChunk;
            [ReadOnly]public ArchetypeChunkComponentType<LocalToWorld> PositionChunk;
            [ReadOnly] [NativeDisableParallelForRestriction]public ComponentDataFromEntity<LocalToWorld> EntityPositions;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<StayInRange> Stay = chunk.GetNativeArray(StayChunk);
                NativeArray<LocalToWorld> Positions = chunk.GetNativeArray(PositionChunk);
                NativeArray<FollowEntityTag> Followers = chunk.GetNativeArray(FollowerChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    StayInRange stay = Stay[i];
                    
                    stay.DistanceToLeader = Vector3.Distance(Positions[i].Position,EntityPositions[Followers[i].Leader].Position);

                    Stay[i] = stay;
                }
            }
        }
    }

}