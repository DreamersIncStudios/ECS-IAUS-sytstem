using System.Collections.Generic;
using Stats.Entities;
using Dreamers.InventorySystem.Interfaces;
using Sirenix.Serialization;
using UnityEngine;
using Stats;
using Dreamers.InventorySystem.AbilitySystem;
using Unity.Entities;
using Unity.Collections;

namespace Dreamers.InventorySystem.Base {
    [System.Serializable]
    public class EquipmentBase
    {
        public Dictionary<ArmorType, ArmorSO> EquippedArmor = new Dictionary<ArmorType, ArmorSO>();
        public Dictionary<WeaponSlot, WeaponSO> EquippedWeapons = new Dictionary<WeaponSlot, WeaponSO>();
        public List<AbilitySO>EquippedAbility = new List<AbilitySO>();

        public int CurrentActivationPoints;
        public int MaxActivationPoints;
        public List<ItemSlot> QuickAccessItems;
        public int NumOfQuickAccessSlots;
        public bool OpenSlots { get { return QuickAccessItems.Count < NumOfQuickAccessSlots; } }

        public void Init() { 
            QuickAccessItems= new List<ItemSlot>();
            NumOfQuickAccessSlots= 2;
            EquippedAbility = new List<AbilitySO>();
        }
        public void Init(EquipmentSave save, BaseCharacterComponent player, Entity entity, int size =2) {
            EquippedArmor = new Dictionary<ArmorType, ArmorSO>();
            EquippedWeapons = new Dictionary<WeaponSlot, WeaponSO>();
            EquippedAbility = new List<AbilitySO>();
            QuickAccessItems = new List<ItemSlot>();
            NumOfQuickAccessSlots=  size;
           LoadEquipment(player, entity, save);
            

        }
        public EquipmentSave Save;
        public void Init(EquipmentSave save, int size = 2)
        {
            EquippedArmor = new Dictionary<ArmorType, ArmorSO>();
            EquippedWeapons = new Dictionary<WeaponSlot, WeaponSO>();
            QuickAccessItems = new List<ItemSlot>();
            NumOfQuickAccessSlots = size;
         Save = save;
            EquippedAbility = new List<AbilitySO>();

        }
        void reloadEquipment(BaseCharacterComponent player) {
            foreach (ArmorSO so in EquippedArmor.Values) {
                so.Equip(player);
            }
            foreach (WeaponSO so in EquippedWeapons.Values)
            {
                so.Equip(player);
            }
        }

        void LoadEquipment(BaseCharacterComponent PC, Entity entity, EquipmentSave Save)
        {
            if (Save.EquippedArmors.Count != 0)
            {
                foreach (ArmorSO SO in Save.EquippedArmors)
                {
                    if (SO != null)
                    {
                        var copy = Object.Instantiate(SO);
                        copy.Equip(PC);
                        EquippedArmor[copy.ArmorType] = copy;
                    }
                }
            }
            if (Save.EquippedWeapons.Count != 0)
            {
                foreach (WeaponSO SO in Save.EquippedWeapons)
                {
                    if (SO != null)
                    {
                        var copy = Object.Instantiate(SO);
                        copy.Equip(PC);
                        EquippedWeapons[copy.Slot] = copy;
                    }
                }
            }

            if (Save.EquippedAbilites.Count != 0) {
                foreach (AbilitySO ability in Save.EquippedAbilites) {
                    ability.EquipAbility(entity);
                }
            }
        }

    }
    [System.Serializable]
    public class EquipmentSave
    {
        public List<WeaponSO> EquippedWeapons;
        public List<ArmorSO> EquippedArmors;
        public List<AbilitySO> EquippedAbilites;
    }

}
