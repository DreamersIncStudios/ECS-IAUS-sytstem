﻿using UnityEngine;
using Unity.Entities;

namespace IAUS.ECS.Component
{

    public class Follow : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Leader LeaderEntity;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            FollowEntityTag data = new FollowEntityTag() {
                Leader = LeaderEntity.Self
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
            buffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }
        EntityCommandBufferSystem buffer;

        protected override void OnUpdate()
        {
            EntityCommandBuffer buff = buffer.CreateCommandBuffer();
            Entities.ForEach((Entity entity, Follow call, ref FollowEntityTag tag, ref SetupUpLeaderTag UP) => 
            {
                tag.Leader = call.LeaderEntity.Self;
                buff.RemoveComponent<SetupUpLeaderTag>(entity);
            });
        }
    }

    

}