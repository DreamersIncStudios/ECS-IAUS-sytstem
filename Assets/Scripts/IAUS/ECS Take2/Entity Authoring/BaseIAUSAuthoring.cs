using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace IAUS.ECS2
{
    public class BaseIAUSAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public List<Transform> WayPoints;
        // use Buffer instead 

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            dstManager.AddBuffer<StateBuffer>(entity);
            dstManager.AddComponent<CreateAIBufferTag>(entity);

            dstManager.AddComponent<BaseAI>(entity);
           


        }
    }
    public struct BaseAI : IComponentData
    {
        public StateBuffer CurrentState;
    }

    public struct CreateAIBufferTag : IComponentData { }
}
