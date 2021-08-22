using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Dreamers.SquadSystem;


namespace IAUS.ECS2.Component
{
    public class Grunt : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Entity self { get; private set; }
   

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            self = entity;
            dstManager.AddComponent<GruntEntityTag>(entity);
            dstManager.AddBuffer<Team>(entity);

        }
    }

}