using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Stats;
using Dreamers.InventorySystem.Base;

namespace Dreamers.InventorySystem {
    [System.Serializable]
    [CreateAssetMenu(fileName = "Recovery Item Data", menuName = "Item System/Recovery Item", order = 1)]
    public class RecoveryItemSO : ItemBaseSO, IRecoverItems
    {
  
        [SerializeField]uint _recoverAmount;
        public uint RecoverAmount { get { return _recoverAmount; } }
        [SerializeField] uint _iterations;
        public uint Iterations { get { return _iterations; } }
        [SerializeField] float _freq;
        public float Frequency { get { return _freq; } }
        [SerializeField] RecoverType _recoverWhat;
        public RecoverType RecoverWhat { get { return _recoverWhat; } }
        [SerializeField] StatusEffect _removeStatus;
        public StatusEffect RemoveStatus { get { return _removeStatus; } }
        public override void Use(InventoryBase inventoryBase, int IndexOf, BaseCharacter player)
        {
            Use(inventoryBase, IndexOf);
            switch (RecoverWhat)
            {
                case RecoverType.Health:
         
                    player.IncreaseHealth((int)RecoverAmount, Iterations, Frequency);

                    break;
                case RecoverType.Mana:
                    player.IncreaseMana((int)RecoverAmount, Iterations, Frequency);
                    break;
                case RecoverType.HealthMana:
                    player.IncreaseHealth((int)RecoverAmount, Iterations, Frequency);
                    player.IncreaseMana((int)RecoverAmount, Iterations, Frequency);
                    break;
                case RecoverType.Status:
                    //add logic later
                    break;
                case RecoverType.StatusPlusHealth:
                    //add status logic
                    player.IncreaseHealth((int)RecoverAmount, Iterations, Frequency);
                    break;
                case RecoverType.StatusPlusMana:
                    //add status logic
                    player.IncreaseMana((int)RecoverAmount, Iterations, Frequency);

                    break;
                case RecoverType.All:
                    //add status logic
                    player.IncreaseMana((int)RecoverAmount, Iterations, Frequency);
                    player.IncreaseHealth((int)RecoverAmount, Iterations, Frequency);

                    break;
            }
        }
          

        

        public override void EquipItem(InventoryBase InventoryBase, EquipmentBase Equipment, int IndexOf, BaseCharacter player)
        {

            bool addNewSlot = true; ;
            for (int i = 0; i < Equipment.QuickAccessItems.Count; i++)
            {
                ItemSlot itemInInventory = Equipment.QuickAccessItems[i];
                if (itemInInventory.Item.ItemID == ItemID && itemInInventory.Count < 99)
                {
                    itemInInventory.Count++;
                    addNewSlot = false;
                    RemoveFromInventory(InventoryBase, IndexOf);
                }
                Equipment.QuickAccessItems[i] = itemInInventory;
            }
            if (Equipment.OpenSlots && addNewSlot)
            {
                Equipment.QuickAccessItems.Add(
                    new ItemSlot()
                    {
                        Item = Instantiate(this),
                        Count = 1
                    });
                RemoveFromInventory(InventoryBase, IndexOf);

            }
            else
            if(addNewSlot)
            { Debug.LogWarning("Can't add item to Quick slot"); }
        }

        public override void Unequip(InventoryBase Inventory, EquipmentBase Equipment, BaseCharacter player, int IndexOf)
        {
            ItemSlot updateItem = Equipment.QuickAccessItems[IndexOf];
            if (Stackable && updateItem.Count > 1)
            {
                updateItem.Count--;
                Equipment.QuickAccessItems[IndexOf] = updateItem;
                updateItem.Item.AddToInventory(Inventory);
            }
            else {
                updateItem.Item.AddToInventory(Inventory);
                Equipment.QuickAccessItems.RemoveAt(IndexOf); }

        }
    }

    public interface IRecoverItems {
        uint RecoverAmount { get; }
        uint Iterations { get; }
        float Frequency { get; }
        RecoverType RecoverWhat { get; }
        StatusEffect RemoveStatus { get; }
    }


    public enum RecoverType{
        Health,Mana, HealthMana, Status, StatusPlusHealth, StatusPlusMana, All
    }
}