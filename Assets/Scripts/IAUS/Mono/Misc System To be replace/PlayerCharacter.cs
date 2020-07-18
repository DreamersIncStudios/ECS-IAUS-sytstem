using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

/*Generic Player Character Class
 * Will be using BurgZergArcade Derived Player Character System  that is already in main project file
 */
namespace Test.CharacterStats
{
    public class PlayerCharacter : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Range(0, 999)]
        [SerializeField] int CurHealth;
        public int curHealth {
            get { return CurHealth; }
            private set { if (value > MaxHealth)
                    CurHealth = MaxHealth;
                else
                    CurHealth = value;
            }
        }

        [Range(0, 999)]
        public int MaxHealth;

        [Range(0, 999)]
        public int CurMana;
        [Range(0, 999)]
        public int MaxMana;
        Entity selfEntityRef;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new Stats() { CurHealth = CurHealth, CurMana = CurMana, MaxHealth = MaxHealth, MaxMana = MaxMana };
            dstManager.AddComponentData(entity, data);
            dstManager.AddComponent<Unity.Transforms.CopyTransformFromGameObject>(entity);
            selfEntityRef = entity;

        }

        private void Start()
        {
            CurHealth = MaxHealth;
            CurMana = MaxMana;
        }
        //need to make a job to account for resistence and level in the future
        public void AdjustHealth(int Change){
            World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(selfEntityRef, new AdjustHealthTag() { value = Change });
        }

        public void AdjustMana(int Change) {
            World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(selfEntityRef, new AdjustManaTag() { value = Change });

        }
    }
    public struct AdjustHealthTag : IComponentData {
        public int value;
    }
    public struct AdjustManaTag : IComponentData
    {
        public int value;
    }
    public class EnemyCharacter : PlayerCharacter, IConvertGameObjectToEntity
    {

    }


    public struct Stats: IComponentData {
        [Range(0, 999)]
        public int CurHealth;
        [Range(0, 999)]
        public int CurMana;
        [Range(0, 999)]
        public int MaxHealth;
        [Range(0, 999)]
        public int MaxMana;
    }

}