
using UnityEngine;
using Unity.Entities;
using Stats;
using UnityEngine.AI;
using Components.MovementSystem;

namespace IAUS.ECS2.Character
{
    public class CharacterAuthoring : MonoBehaviour, IConvertGameObjectToEntity
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
        [Header("Movement Parameter")]
        public float StoppingDistance;
        NavMeshAgent agent;

        public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            agent = this.GetComponent<NavMeshAgent>();
            Movement move = new Movement() { StoppingDistance = StoppingDistance, Acceleration = agent.acceleration, MovementSpeed = agent.speed };
            dstManager.AddComponent<EnemyCharacter>(entity);
           dstManager.AddComponentData(entity, move);
            dstManager.AddComponent<Unity.Transforms.CopyTransformFromGameObject>(entity);

            dstManager.AddComponent<NPC>(entity);
        }
    }

    public struct NPC : IComponentData
    {

        public Entity HomeEntity { get; set; }

    }
}
