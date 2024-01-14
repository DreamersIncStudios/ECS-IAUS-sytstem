using System.Collections.Generic;
using System.Linq;
using Dreamers.InventorySystem.Interfaces;
using Dreamers.InventorySystem.AbilitySystem;
using Sirenix.Utilities;

namespace Dreamers.InventorySystem.Base {
    [System.Serializable]
    public class InventoryBase
    {
        public List<ItemSlot> ItemsInInventory;
        public List<AbilitySO> AbilitiesInInventory;
        public uint MaxInventorySize;
        public bool OverBurdened => ItemsInInventory.Count >= MaxInventorySize;
        public InventoryBase() 
        {
            ItemsInInventory = new List<ItemSlot>();
            AbilitiesInInventory = new List<AbilitySO>();
        }

        public void Init(uint size) {
            MaxInventorySize = size;
        }
        public void Init(InventorySave save) { 
            MaxInventorySize= save.MaxInventorySize;
            if (save.ItemsInInventory.IsNullOrEmpty()) return;
            foreach (var item in save.ItemsInInventory)
            {
                AddToInventory(item);
            }
        }
        //TODO Need to Update for Stackable items;
        //TODO Remove Quest from Inventory System

        public bool OpenSlots(ItemBaseSO item, out ItemSlot itemSlot) {
            if (FindItemSlots(item, out var itemSlots)) 
            {
 
                foreach (var slot in itemSlots)
                {
      
                    if(slot.Filled) continue;
                    itemSlot = slot;
                    return true;
                }
              
            }
            itemSlot = default;
            return false ;
        }
        
        public bool FirstIndexOfItem(int itemID, out ItemSlot returnedItem) {

            var ans =FindItemSlots(itemID, out var itemSlots);
            returnedItem = itemSlots[0];
            return ans;
        }

        public bool FirstIndexOfItem(ItemBaseSO item, out ItemSlot returnedItem)
        {
            return FirstIndexOfItem((int)item.ItemID, out returnedItem);
        }

        public bool FindItemSlots(int ItemID, out List<ItemSlot> item)
        {
            item = ItemsInInventory.Where(itemSlot => itemSlot.Item.ItemID == ItemID).ToList();

            return item.Count>0;
        }

        public bool FindItemSlots(ItemBaseSO item, out List<ItemSlot> returnedItem)
        {
            return FindItemSlots((int)item.ItemID, out returnedItem);
        }

        public bool FindItemSlotIndex(int ItemID, out int indexOf)
        {
            indexOf = -1;
            foreach (var itemSlot in ItemsInInventory.Where(itemSlot => itemSlot.Item.ItemID == ItemID))
            {
                indexOf = ItemsInInventory.IndexOf(itemSlot);
                return true;
            }

            return false;
        }

        public bool FindItemSlotIndex(int ItemID, out ItemSlot slot)
        {
            foreach (var itemSlot in ItemsInInventory.Where(itemSlot => itemSlot.Item.ItemID == ItemID))
            {
                slot = itemSlot;
                return true;
            }
            slot = default;
            return false;
        }

        public List<ItemSlot> GetItemsByType(ItemType Type) {
            var ItemByType = new List<ItemSlot>();
            foreach(var Slot in ItemsInInventory)
            {
                if(Type == ItemType.None)
                    ItemByType.Add(Slot);
                else if (Slot.Item.ItemType == Type)
                {
                    ItemByType.Add(Slot);
                }
            }
                return ItemByType;
        }

        public InventorySave GetInventorySave() {
            var save = new InventorySave(ItemsInInventory,MaxInventorySize );
            save.MaxInventorySize = MaxInventorySize;
            save.ItemsInInventory = ItemsInInventory;
            return save;
        }

        public void LoadInventory(InventorySave inventorySave) {
            MaxInventorySize = inventorySave.MaxInventorySize;
            ItemsInInventory = inventorySave.ItemsInInventory;
        }

        void AddToInventory(ItemSlot itemSlot)
        {
            ItemsInInventory.Add(itemSlot);
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
                        var indexOf = ItemsInInventory.IndexOf(itemSlot);
                        itemSlot.Count++;
                        ItemsInInventory[indexOf] = itemSlot;
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
            if (item.ItemType == ItemType.Quest) {
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
        public bool Filled => Item.MaxStackCount <= Count;
    }
    [System.Serializable]
    public class InventorySave {

        //Todo Add funtionality to load
        public List<ItemSlot> ItemsInInventory;
        public uint MaxInventorySize;

        public InventorySave(List<ItemSlot> itemsInInventory, uint maxInventorySize)
        {
            this.MaxInventorySize = maxInventorySize;
            this.ItemsInInventory = itemsInInventory;
        }
    }
}