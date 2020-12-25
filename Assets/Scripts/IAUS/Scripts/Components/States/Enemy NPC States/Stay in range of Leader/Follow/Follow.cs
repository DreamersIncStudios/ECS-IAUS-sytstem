using UnityEngine;
using Unity.Entities;

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

    

}