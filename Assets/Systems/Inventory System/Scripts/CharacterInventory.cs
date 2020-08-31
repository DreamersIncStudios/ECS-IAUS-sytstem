using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamers.InventorySystem.Base;
using Stats;

namespace Dreamers.InventorySystem
{
    public class CharacterInventory : MonoBehaviour
    {

        public InventoryBase Inventory;
        public EquipmentBase Equipment;

        public int Gold;

        void Awake() {
            Inventory = new InventoryBase(this.GetComponent<BaseCharacter>());
            Equipment = new EquipmentBase();
        
        }

        // Start is called before the first frame update

    }
}