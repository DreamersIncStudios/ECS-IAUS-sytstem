using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using  InfluenceSystem.Component;
using Unity.Mathematics;
namespace IAUS.ECS2.Component
{
    
    public class Leader : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Entity Self { get; private set; }
        public Influence influence;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            Self = entity;
            dstManager.AddComponent<LeaderEntityTag>(entity);
            dstManager.AddComponentData(entity, influence);
        }   
   
    }

    public struct LeaderEntityTag:IComponentData { }

}