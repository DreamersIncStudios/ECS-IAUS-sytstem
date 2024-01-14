using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using Dreamers.Global;
#endif

namespace Dreamers.InventorySystem.AbilitySystem
{
    public abstract class AbilitySO : ScriptableObject
    {
        [SerializeField] public uint ID { get; private set; }
        public string Name { get { return nameSO; } }
        [SerializeField] string nameSO;
        public string Description { get { return description; } }
        [SerializeField] string description;
        public string InputCode { get { return inputCode; } }
        [SerializeField] string inputCode;
        public AbilityAnimationInfo AnimInfo { get { return animInfo; } }
        [SerializeField] AbilityAnimationInfo animInfo;
        public void SetID(uint id) { 
            this.ID = id;
        }
        public abstract void Activate(Entity CasterEntity);

        public virtual void EquipAbility(Entity CasterEntity) {
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var inventory = em.GetComponentData<CharacterInventory>(CasterEntity);
            inventory.Equipment.EquippedAbility.Add(ScriptableObject.Instantiate( this));
            inventory.Inventory.AbilitiesInInventory.Remove(this);
            em.AddComponentData(CasterEntity, new UpdateCommandHandlerTag());
        }
        public void UnequipAbility(Entity CasterEntity)
        {
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var inventory = em.GetComponentData<CharacterInventory>(CasterEntity);
            if(inventory.Equipment.EquippedAbility.Contains(this)) {
                inventory.Inventory.AbilitiesInInventory.Add(Instantiate( this));
                inventory.Equipment.EquippedAbility.Remove(this);
                em.AddComponentData(CasterEntity, new UpdateCommandHandlerTag());

            }
        }
    }


    public static class AbilityDatabase
    {
        static public List<AbilitySO> abilities;
        static public bool isLoaded { get; private set; }
        private static void ValidateDatabase() {
            if (abilities == null || isLoaded)
            {
                abilities = new List<AbilitySO>();
                isLoaded = false;
            }
            else { isLoaded = true; }
        }
        public static void LoadDatabase() {
            if (isLoaded)
                return;
            LoadDatabaseForce();
        }

        public static void LoadDatabaseForce()
        {
            abilities = new List<AbilitySO>();
            isLoaded = true;
            AbilitySO[] abilitiesToLoad = Resources.LoadAll<AbilitySO>(@"Ability");
            foreach (AbilitySO ability in abilitiesToLoad) {
                if (!abilities.Contains(ability))
                    abilities.Add(ability);
            }
        }
        public static void ClearDatabase() {
            isLoaded = false;
            abilities.Clear();
        }

        public static AbilitySO GetAbility(uint id) {
            ValidateDatabase();
            LoadDatabase();
            foreach (AbilitySO ability in abilities)
            {
                if (ability.ID == id)
                    return ScriptableObject.Instantiate(ability);

            }
            return null;
        }

#if UNITY_EDITOR
        [MenuItem("Assets/Create/Ability System/Recovery")]
        static public void CreateRecoveryAbility() {
            ScriptableObjectUtility.CreateAsset<RecoveryMagic>(@"Ability", out RecoveryMagic magic);
            LoadDatabaseForce();
            magic.SetID((uint)abilities.Count + 1);
            Debug.Log($"Ability ID {magic.ID} was created");
        }
        [MenuItem("Assets/Create/Ability System/Attack")]
        static public void CreateAttackAbility()
        {
            ScriptableObjectUtility.CreateAsset<AttackMagic>(@"Ability", out AttackMagic magic);
            LoadDatabaseForce();
            magic.SetID((uint)abilities.Count + 1);
            Debug.Log($"Ability ID {magic.ID} was created");
        }
#endif
    }
  


    public class AbilityList
    {
        public List<AbilitySO> EquippedAbilities { get; set; }

        public AbilitySO GetAbility(string inputCode) {
            if (EquippedAbilities == null)
                return null;
            foreach (AbilitySO ability in EquippedAbilities)
            {
                if (ability.InputCode.Equals(inputCode))
                    return ability;
            }
            return null;
        }
    }
     public enum AbilityType
    {
        Spell, 
        Attack,
        Summon,
        Taunt, 
        ProjectileSpell,
    }
    [System.Serializable]
    public struct AbilityAnimationInfo {
        public uint AnimIndex;
        public float TransitionDuration, TransitionOffset, EndofCurrentAnim;

    }

    public struct UpdateCommandHandlerTag:IComponentData { }
}
