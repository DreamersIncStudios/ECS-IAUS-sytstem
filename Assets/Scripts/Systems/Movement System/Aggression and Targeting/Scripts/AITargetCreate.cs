using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Global.Component
{
    /// <summary>
    ///  This creates an AI Target Compoment and get sets Get InstanceID var 
    ///  DO not use AITarget on GameObject.
    ///  Removed [GenerateAuthoring] tag
    /// </summary>
    public class AITargetCreate : MonoBehaviour, IConvertGameObjectToEntity
    {
        public AITarget aITarget;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            aITarget.GetInstanceID = gameObject.GetInstanceID();
            dstManager.AddComponentData(entity, aITarget);
        }

    }
}
