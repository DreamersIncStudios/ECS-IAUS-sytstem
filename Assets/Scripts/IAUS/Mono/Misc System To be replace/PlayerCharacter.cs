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
        public int CurHealth;
        [Range(0, 999)]
        public int CurMana;
        [Range(0, 999)]
        public int MaxHealth;
        [Range(0, 999)]
        public int MaxMana;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new Stats() { CurHealth = CurHealth, CurMana = CurMana, MaxHealth = MaxHealth, MaxMana = MaxMana };
            dstManager.AddComponentData(entity, data);
            dstManager.AddComponent<Unity.Transforms.CopyTransformFromGameObject>(entity);

        }

        private void Start()
        {
            CurHealth = MaxHealth;
            CurMana = MaxMana;
        }
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