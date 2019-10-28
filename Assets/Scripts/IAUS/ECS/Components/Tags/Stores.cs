using Unity.Entities;
using UnityEngine;
using IAUS.ECS.Component;

namespace IAUS.Sample.Archtypes.Tags
{
    public class Stores : MonoBehaviour,IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var store = new StoreC();
            dstManager.AddComponentData(entity, store);
        }
    }
}

namespace IAUS.ECS.Component
{
    public struct StoreC : IComponentData { }
}