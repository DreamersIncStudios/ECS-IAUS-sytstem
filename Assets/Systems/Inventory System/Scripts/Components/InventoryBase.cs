using System.Collections.Generic;
using UnityEngine;
using Dreamers.InventorySystem.Interfaces;
namespace Dreamers.InventorySystem.Base {
    [System.Serializable]
    public class InventoryBase
    {
        public List<ItemSlot> ItemsInInventory;
        public uint MaxInventorySize;
        public bool OverBurdened => ItemsInInventory.Count >= MaxInventorySize;
        public InventoryBase() 
        {
            ItemsInInventory = new List<ItemSlot>();
        }

        public void Init(uint size) {
            MaxInventorySize = size;
        }
        public void Init(InventoryBase save) { 
            MaxInventorySize= save.MaxInventorySize;
            ItemsInInventory= save.ItemsInInventory;
        }
        //TODO Need to Update for Stackable items;
        //TODO Remove Quest from Inventory System

        public bool OpenSlots(ItemBaseSO item, out ItemSlot itemSlot) {
            itemSlot = new ItemSlot();
            if (FindItemSlots(item, out List<ItemSlot> itemSlots)) { }
            {
                foreach (var slot in itemSlots)
                {
                    if (!slot.Filled)
                    {
                        itemSlot = slot;
                        return true;
                    }
                }
            }
                return false ;


        }
        public bool FirstIndexOfItem(int ItemID, out ItemSlot returnedItem) {

            bool ans =FindItemSlots(ItemID, out List<ItemSlot> itemSlots);
            returnedItem = itemSlots[0];
            return ans;
        }

        public bool FirstIndexOfItem(ItemBaseSO item, out ItemSlot returnedItem)
        {
            return FirstIndexOfItem((int)item.ItemID, out returnedItem);
        }

        public bool FindItemSlots(int ItemID, out List<ItemSlot> item)
        {
            item = new List<ItemSlot>();
            foreach (ItemSlot itemSlot in ItemsInInventory)
            {
                if (itemSlot.Item.ItemID == ItemID)
                {
                   item.Add(itemSlot);
                   
                }
            }

            return item.Count>0;
        }

        public bool FindItemSlots(ItemBaseSO item, out List<ItemSlot> returnedItem)
        {
            return FindItemSlots((int)item.ItemID, out returnedItem);
        }

        public bool FindItemSlotIndex(int ItemID, out int indexOf)
        {
            indexOf = -1;
            foreach (ItemSlot itemSlot in ItemsInInventory)
            {
                if (itemSlot.Item.ItemID == ItemID)
                {
                    indexOf = ItemsInInventory.IndexOf(itemSlot);
                    return true;
                }
            }

            return false;
        }

        public bool FindItemSlotIndex(int ItemID, out ItemSlot slot)
        {
            foreach (ItemSlot itemSlot in ItemsInInventory)
            {
                if (itemSlot.Item.ItemID == ItemID)
                {
                    slot = itemSlot;
                    return true;
                }
            }
            slot = default;
            return false;
        }

        public List<ItemSlot> GetItemsByType(ItemType Type) {
            List<ItemSlot> ItemByType = new List<ItemSlot>();
            foreach(ItemSlot Slot in ItemsInInventory)
            {
                if(Type == ItemType.None)
                    ItemByType.Add(Slot);
                else if (Slot.Item.Type == Type)
                {
                    ItemByType.Add(Slot);
                }
            }
                return ItemByType;
        }

        public InventorySave GetInventorySave() {
            InventorySave Save = new InventorySave();
            Save.MaxInventorySize = MaxInventorySize;
            Save.ItemsInInventory = ItemsInInventory;
            return Save;
        }

        public void LoadInventory(InventorySave inventorySave) {
            MaxInventorySize = inventorySave.MaxInventorySize;
            ItemsInInventory = inventorySave.ItemsInInventory;
        }
        public bool AddToInventory(ItemBaseSO item ) {
            if (OverBurdened && OpenSlots(item, out _))
                return false;
            else
            {
                if (item.Stackable)
                {
                    if (OpenSlots(item, out ItemSlot itemSlot))
                    {
                        itemSlot.Count++;

                        return true;
                    }
                    else {
                        AddNew(item);
                            return true;
                    }
                }
                else {
                    AddNew(item);
                    return true;
                }
            }
        }
        private void AddNew(ItemBaseSO item) {
            ItemsInInventory.Add( new ItemSlot()
            {
                Item = item,
                Count = 1
            });
        
        }

        //public bool AddToInventory(int itemID) {
        //   return AddToInventory( ItemDatabase.GetItem(itemID));
        //}

        //public bool RemoveFromInventory(int index) {
        //    return RemoveFromInventory(ItemDatabase.GetItem(index));
        //}
        public bool RemoveFromInventory(ItemBaseSO item) {
            if (item.Type == ItemType.Quest) {
                return false;
            }
            else
            {
                ForceRemoveFromInventory(item);
                return true;
            }
        
        }

        public void ForceRemoveFromInventory(ItemBaseSO item) {
            if (item.Stackable)
            {
                FirstIndexOfItem(item, out ItemSlot slot);
                int index = ItemsInInventory.IndexOf(slot);
                slot.Count--;
                if (slot.Count <= 0)
                {
                    ItemsInInventory.RemoveAt(index);
                }
                else
                {
                    ItemsInInventory[index] = slot;
                }
            }
            else
            {
                FindItemSlotIndex((int)item.ItemID, out int index);
                ItemsInInventory.RemoveAt(index);

            }

        }

        public bool OpenSlot { get { return ItemsInInventory.Count < MaxInventorySize; } }
   
    }

    [System.Serializable]
    public struct ItemSlot{
        public ItemBaseSO Item;
        public int Count;
        public bool Filled => Item.MaxStackCount >= Count;
    }
    [System.Serializable]
    public class InventorySave {
        public List<ItemSlot> ItemsInInventory;
        public uint MaxInventorySize;
    }
}