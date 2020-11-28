using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace DreamersStudio.TargetingSystem
{
    public class TargetableAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public TargetType TargetType;
        public int ID;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            ID = gameObject.GetInstanceID();
            var data = new Targetable()
            {
                TargetType = TargetType,
                ID = ID
            };
            dstManager.AddComponentData(entity, data);

        }



    }
}
