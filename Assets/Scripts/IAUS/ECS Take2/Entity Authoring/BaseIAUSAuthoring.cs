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
           // dstManager.AddComponent<HealthConsideration>(entity);
           // dstManager.AddComponent<DistanceToConsideration>(entity);
           // dstManager.AddComponent<TimerConsideration>(entity);

            //DynamicBuffer<PatrolBuffer> buffer = dstManager.GetBuffer<PatrolBuffer>(entity);
            DynamicBuffer<StateBuffer> buffer2 = dstManager.GetBuffer<StateBuffer>(entity);

            //foreach (Transform point in WayPoints)
            //{
            //    buffer.Add(new PatrolBuffer() { Point = point.position });
            //}

            buffer2.Add(new StateBuffer()
            {
                StateName = AIStates.Wait,
                Status = ActionStatus.Idle
            });

            //buffer2.Add(new StateBuffer()
            //{
            //    StateName = AIStates.Patrol,
            //    Status = ActionStatus.Idle
            //});


        }
    }
    public struct BaseAI : IComponentData
    {
        public StateBuffer CurrentState;
    }

    public struct CreateAIBufferTag : IComponentData { }
}
