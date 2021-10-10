using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Dreamers.SquadSystem;
namespace IAUS.ECS.Component
{
    
    public class Leader : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Entity Self { get; private set; }
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            Self = entity;
            dstManager.AddComponent<LeaderEntityTag>(entity);
            dstManager.AddBuffer<Team>(entity);

        }

    }



}