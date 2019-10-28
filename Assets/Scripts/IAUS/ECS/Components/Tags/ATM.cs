
using UnityEngine;
using IAUS.ECS.Component;
using Unity.Entities;
namespace IAUS.Sample.Archtypes.Tags
{
    public class ATM : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var ATMS = new ATMC();
            dstManager.AddComponentData(entity, ATMS);
        }
    }
}
namespace IAUS.ECS.Component
{
    public struct ATMC : IComponentData { }
}
