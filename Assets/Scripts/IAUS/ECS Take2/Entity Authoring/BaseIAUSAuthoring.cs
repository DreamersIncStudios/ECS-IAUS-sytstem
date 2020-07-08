using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace IAUS.ECS2
{
    public class BaseIAUSAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        // use Buffer instead 

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            dstManager.AddBuffer<StateBuffer>(entity);
            dstManager.AddComponent<CreateAIBufferTag>(entity);
            var addAI = new BaseAI()
            {
                CurrentState = new StateBuffer()
                {
                    StateName = AIStates.none,
                    Status = ActionStatus.Failure
                }
            };

            dstManager.AddComponentData(entity,addAI);
           


        }
    }
    public struct BaseAI : IComponentData
    {
        public StateBuffer CurrentState;
        public bool Set;
    }

    public struct CreateAIBufferTag : IComponentData { }
}
