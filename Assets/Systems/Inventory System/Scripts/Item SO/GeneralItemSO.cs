using Dreamers.InventorySystem.Base;
using Stats;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamers.InventorySystem
{
    [System.Serializable]
    public class GeneralItemSO : ItemBaseSO, IGeneral
    {
        [SerializeField] private TypeOfGeneralItem _GeneralType;
        public TypeOfGeneralItem GeneralItemType { get { return _GeneralType; } }

        public override void EquipItem(InventoryBase inventoryBase, EquipmentBase Equipment, int IndexOf, BaseCharacter player)
        {
           
        }

        public override void Unequip(InventoryBase inventoryBase, EquipmentBase Equipment, BaseCharacter player, int IndexOf)
        {
         
        }

        public override void Use(InventoryBase inventoryBase, int IndexOf, BaseCharacter player)
        {
      
        }
    }
}
