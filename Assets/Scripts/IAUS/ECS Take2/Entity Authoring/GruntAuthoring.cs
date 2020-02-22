using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using CharacterStats;

namespace IAUS.ECS2
{
    public class GruntAuthoring : MonoBehaviour,IConvertGameObjectToEntity
    {
        [Header("Character Stats")]
        [Range(0, 999)]
        public int CurHealth;
        [Range(0, 999)]
        public int CurMana;
        [Range(0, 999)]
        public int MaxHealth;
        [Range(0, 999)]
        public int MaxMana;

        public List<Transform> WayPoints;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
           

            dstManager.AddComponent<HealthConsideration>(entity);
            dstManager.AddComponent<DistanceToConsideration>(entity);
            dstManager.AddComponent<TimerConsideration>(entity);
            dstManager.AddComponent<EnemyCharacter>(entity);
            dstManager.AddBuffer<PatrolBuffer>(entity);
            dstManager.AddBuffer<StateBuffer>(entity);
            dstManager.AddComponent<TestAI>(entity);
            dstManager.AddComponent<ECS.Component.Movement>(entity);
            var data = new Stats() { CurHealth = CurHealth, CurMana = CurMana, MaxHealth = MaxHealth, MaxMana = MaxMana };
            dstManager.AddComponentData(entity, data);


            DynamicBuffer<PatrolBuffer> buffer = dstManager.GetBuffer<PatrolBuffer>(entity);
            DynamicBuffer<StateBuffer> buffer2 = dstManager.GetBuffer<StateBuffer>(entity);

            foreach (Transform point in WayPoints)
            {
                buffer.Add(new PatrolBuffer() { Point = point.position });
            }
            buffer2.Add(new StateBuffer()
            {
                StateName = AIStates.Wait,
                Status = ActionStatus.Idle
            });

            buffer2.Add(new StateBuffer()
            {
                StateName = AIStates.Patrol,
                Status = ActionStatus.Idle
            });


        }
    
    }
    public struct TestAI : IComponentData
    {
        public StateBuffer CurrentState;
    }
}