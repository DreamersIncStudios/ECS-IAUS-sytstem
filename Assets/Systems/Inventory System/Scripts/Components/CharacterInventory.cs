using Dreamers.InventorySystem.Base;
using Stats;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace Dreamers.InventorySystem
{
    public class CharacterInventory : IComponentData
    {
        public int Gold { get; private set; }

        public InventoryBase Inventory;
        public EquipmentBase Equipment;

        public void Setup() {
            Inventory = new ();
            Inventory.Init(10);
            Equipment = new();
            Equipment.Init();

        }
        public void Setup(InventoryBase inventory)
        {
            Inventory = new();
            Inventory.Init(inventory);
            Equipment = new();
            Equipment.Init();

        }
        public void Setup(EquipmentSave equipment, BaseCharacterComponent player)
        {
            Inventory = new();
            Inventory.Init(10);
            Equipment = new();
            Equipment.Init(equipment, player);

        }
        public void Setup(InventoryBase inventory, EquipmentSave equipment, BaseCharacterComponent player)
        {
            Inventory = new();
            Inventory.Init(inventory);
            Equipment = new();
            Equipment.Init(equipment,player);
        }

        public void RemoveGold(uint amount) {

            if (amount <= Gold)
            {
                Gold = (int)Mathf.Clamp(Gold - amount, 0, Mathf.Infinity);
            }
        }
        public void AddGold(uint amount)
        {
                Gold = (int)Mathf.Clamp(Gold + amount, 0, Mathf.Infinity);
        }
    }
    public class CharacterInvenetoryComp : IComponentData {
        public EquipmentBase equipment;
        public InventoryBase inventory;
    }

}