using Dreamers.InventorySystem.Base;
using Stats;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Dreamers.InventorySystem.Interfaces;
using Unity.Entities;
using UnityEngine;

namespace Dreamers.InventorySystem
{
    public class CharacterInventory : IComponentData
    {
        public uint Gold { get; private set; }

        public InventoryBase Inventory;
        public EquipmentBase Equipment;
        
        public void Setup()
        {
            Inventory = new();
            Inventory.Init(10);
            Equipment = new();
            Equipment.Init();

        }
        public void Setup(InventorySave inventory)
        {
            Inventory = new();
            Inventory.Init(inventory);
            Equipment = new();
            Equipment.Init();

        }
        public void Setup(Entity entity, EquipmentSave equipment, BaseCharacterComponent player)
        {
            Inventory = new();
            Inventory.Init(10);
            Equipment = new();
            Equipment.Init(equipment, player, entity);

        }
        public void Setup(Entity entity, InventorySave inventory, EquipmentSave equipment, BaseCharacterComponent player)
        {
            Inventory = new();
            Inventory.Init(inventory);
            Equipment = new();
            Equipment.Init(equipment, player, entity);
        }

        public void RemoveGold(uint amount)
        {

            if (amount <= Gold)
            {
                Gold = (uint)Mathf.Clamp(Gold - amount, 0, Mathf.Infinity);
            }
        }
        public void AddGold(uint amount)
        {
            Gold = (uint)Mathf.Clamp(Gold + amount, 0, Mathf.Infinity);
        }

        public bool BuyItem(ItemBaseSO item)
        {
            if (item.Value < Gold &&
                Inventory.AddToInventory(ScriptableObject.Instantiate(item)))
            {
                RemoveGold(item.Value);
                return true;
            }
            else
                return false;
        }

        public bool SellItem(ItemBaseSO item)
        {
            if (!item.QuestItem)
            {
                Inventory.RemoveFromInventory(item);
                AddGold(item.Value);
                return true;
            }
            else return false;
        }
    }
    public struct CheckAttackStatus : IComponentData { }
}