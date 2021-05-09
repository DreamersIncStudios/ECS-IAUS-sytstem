using System.Collections.Generic;
using UnityEngine;
namespace Dreamers.InventorySystem.Base { 

    [System.Serializable]
public class EquipmentBase
    {
        public WeaponSO PrimaryWeapon;
        public WeaponSO SecondaryWeapon;
        public WeaponSO ProjectileWeopon;

        public ArmorSO Shield;
        public ArmorSO Helmet;
        public ArmorSO Chest;
        public ArmorSO Arms;
        public ArmorSO Legs;
        public ArmorSO Signature;


        public List<ItemSlot> QuickAccessItems;
        public int NumOfQuickAccessSlots;
        public bool OpenSlots { get { return QuickAccessItems.Count < NumOfQuickAccessSlots; } }
 
    }   

}
