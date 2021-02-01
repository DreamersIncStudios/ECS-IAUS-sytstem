using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using InfluenceSystem.Component;

namespace IAUS.ECS2.Component
{
    public class Grunt : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Entity self { get; private set; }
        public Influence influence;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            self = entity;
            dstManager.AddComponentData(entity, influence);
            dstManager.AddComponent<GruntEntityTag>(entity);
        }
    }

    public struct GruntEntityTag : IComponentData { }
}