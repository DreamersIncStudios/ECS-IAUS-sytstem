using Unity.Entities;
using UnityEngine;
using IAUS.ECS.Component;
using Global.Component;
using Components.MovementSystem;

public class BaseAIAuthoring : MonoBehaviour,IConvertGameObjectToEntity
{
    public AITarget Self;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity,Self);
        dstManager.AddComponent<IAUSBrain>(entity);
        dstManager.AddComponent<SetupBrainTag>(entity);
        dstManager.AddBuffer<StateBuffer>(entity);



    }

}
