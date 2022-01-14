 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace EssentialConversion
{
    public class EntityConversionComponent : MonoBehaviour,IConvertGameObjectToEntity
    {
        public Entity ObjectEntity;
        public EntityManager Manager;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            ObjectEntity = entity;
            Manager = dstManager;
        }

        private void OnDestroy()
        {

        }

        private void OnDisable()
        {
            //if (Manager != null && Manager.IsCreated && Manager.Exists(ObjectEntity))
            //    Manager.DestroyEntity(ObjectEntity);

         //  Manager = null;
         //  ObjectEntity = Entity.Null;
        }

        private void OnEnable()
        {

            //if (Manager != null && Manager.IsCreated && !Manager.Exists(ObjectEntity))
            //    ConvertToEntity.ConvertAndInjectOriginal(gameObject);

        }

    }
}