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
            dstManager.AddComponent<TestAI>(entity);

            dstManager.AddComponent<HealthConsideration>(entity);
            dstManager.AddComponent<DistanceToConsideration>(entity);
            dstManager.AddComponent<TimerConsideration>(entity);
            dstManager.AddComponent<EnemyCharacter>(entity);
            dstManager.AddBuffer<PatrolBuffer>(entity);
            var data = new Stats() { CurHealth = CurHealth, CurMana = CurMana, MaxHealth = MaxHealth, MaxMana = MaxMana };
            dstManager.AddComponentData(entity, data);


            DynamicBuffer<PatrolBuffer> buffer = dstManager.GetBuffer<PatrolBuffer>(entity);

            foreach (Transform point in WayPoints)
            {
                buffer.Add(new PatrolBuffer() { Point = point.position });
            }




        }
    
    }
    public struct TestAI : IComponentData
    {
        public float Patrol;
        public float wait;

    }
}