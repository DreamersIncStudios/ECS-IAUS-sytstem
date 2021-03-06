﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace IAUS.ECS2.Component
{
    
    public class Leader : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Entity self { get; private set; }
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            self = entity;
            dstManager.AddComponent<LeaderEntityTag>(entity);
        }

    }

    public struct LeaderEntityTag:IComponentData { }

}