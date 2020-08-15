using System.Collections.Generic;


namespace Dreamers.InventorySystem.Base {
    [System.Serializable]
    public class InventoryBase
    {
        public List<ItemSlot> ItemsInInventory;
        public uint MaxInventorySize;

        // Need to Update for Stackable items;
        public bool OpenSlots(ItemSlot Slot) {
            if (Slot.Item.Stackable) 
            {
                for (int i = 0; i < ItemsInInventory.Count; i++)
                {
                    ItemSlot itemInInventory = ItemsInInventory[i];
                    if (itemInInventory.Item.ItemID == Slot.Item.ItemID && itemInInventory.Count < 99)
                    {
                        return true;

                    }
                    if (itemInInventory.Item.ItemID == Slot.Item.ItemID && itemInInventory.Count == 99)
                    {
                        return ItemsInInventory.Count < MaxInventorySize;
                    }
                }
                return false;
            }
            else
            return ItemsInInventory.Count < MaxInventorySize;  }

        public bool OpenSlot { get { return ItemsInInventory.Count < MaxInventorySize; } }
        public int Gold;



    }

    [System.Serializable]
    public struct ItemSlot{
        public ItemBaseSO Item;
        public int Count;
    }

}