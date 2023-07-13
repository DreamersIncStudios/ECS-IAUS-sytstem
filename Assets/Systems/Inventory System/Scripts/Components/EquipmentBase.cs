using System.Collections.Generic;
using Stats.Entities;
using Dreamers.InventorySystem.Interfaces;
using Sirenix.Serialization;
using UnityEngine;
using Stats;

namespace Dreamers.InventorySystem.Base {
    [System.Serializable]
    public class EquipmentBase
    {
        public Dictionary<ArmorType, ArmorSO> EquippedArmor = new Dictionary<ArmorType, ArmorSO>();
        public Dictionary<WeaponSlot, WeaponSO> EquippedWeapons = new Dictionary<WeaponSlot, WeaponSO>();


        public int CurrentActivationPoints;
        public int MaxActivationPoints;
        public List<ItemSlot> QuickAccessItems;
        public int NumOfQuickAccessSlots;
        public bool OpenSlots { get { return QuickAccessItems.Count < NumOfQuickAccessSlots; } }

        public void Init() { 
            QuickAccessItems= new List<ItemSlot>();
            NumOfQuickAccessSlots= 2;
        }
        public void Init(EquipmentSave save, BaseCharacterComponent player, int size =2) {
            EquippedArmor = new Dictionary<ArmorType, ArmorSO>();
            EquippedWeapons = new Dictionary<WeaponSlot, WeaponSO>(); 
            QuickAccessItems= new List<ItemSlot>();
            NumOfQuickAccessSlots=  size;
           LoadEquipment(player,save);
        }
        public EquipmentSave Save;
        public void Init(EquipmentSave save, int size = 2)
        {
            EquippedArmor = new Dictionary<ArmorType, ArmorSO>();
            EquippedWeapons = new Dictionary<WeaponSlot, WeaponSO>();
            QuickAccessItems = new List<ItemSlot>();
            NumOfQuickAccessSlots = size;
         Save = save;
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

        void LoadEquipment(BaseCharacterComponent PC, EquipmentSave Save)
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
        }

    }
    [System.Serializable]
    public class EquipmentSave
    {
        public List<WeaponSO> EquippedWeapons;
        public List<ArmorSO> EquippedArmors;
    }

}
