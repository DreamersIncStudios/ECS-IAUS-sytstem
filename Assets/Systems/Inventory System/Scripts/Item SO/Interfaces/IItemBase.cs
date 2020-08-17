using Unity.Entities;
using UnityEngine;
using Stats;
using Dreamers.InventorySystem.Base;

namespace Dreamers.InventorySystem
{

    public interface IItemBase
    {
        uint ItemID { get; set; }
        string ItemName { get; }
        string Description {get;}
        Sprite Icon { get; }
        int Value { get; }
        ItemType Type { get; }
        bool Stackable { get; }
        bool Disposible { get; }
        bool QuestItem { get; }

        void Use(InventoryBase inventoryBase, int IndexOf);

        void AddToInventory(InventoryBase inventory);
        void RemoveFromInventory(InventoryBase inventory, int IndexOf);

    }
    [System.Serializable]
    public abstract class ItemBaseSO : ScriptableObject, IItemBase
    {
  
        public uint ItemID { get; set; } // To be implemented with Database system/CSV Editor creator 
        [SerializeField] private string _itemName;
        public string ItemName { get { return _itemName; } }
        [TextArea(3,6)]
        [SerializeField] private string _desc;
        public string Description { get { return _desc; } }
        [SerializeField] private Sprite _icon;
        public Sprite Icon { get { return _icon; } }

        [SerializeField] private int _value;
        public int Value { get { return _value; } }
        [SerializeField] private ItemType _type;
        public ItemType Type { get { return _type; } }
        [SerializeField]  private bool _stackable;
        public bool Stackable { get { return _stackable; } }
        //[SerializeField] bool _disposible;
        public bool Disposible { get { return !QuestItem; } }
        [SerializeField] bool _questItem;
        public bool QuestItem { get { return _questItem; } }

        public  void Use(InventoryBase inventoryBase, int IndexOf)
        {
            RemoveFromInventory(inventoryBase, IndexOf);

        }
        public abstract void Use(InventoryBase inventoryBase, int IndexOf, BaseCharacter player);

        //How we handle Equip Potions
        public abstract void EquipItem(InventoryBase inventoryBase, EquipmentBase Equipment,int IndexOf, BaseCharacter player);
        public abstract void Unequip(InventoryBase inventoryBase, EquipmentBase Equipment, BaseCharacter player, int IndexOf);
        
        public virtual void AddToInventory(InventoryBase inventory)
        {
            bool addNewSlot = true; ;
            for (int i = 0; i < inventory.ItemsInInventory.Count; i++)
            {
                ItemSlot itemInInventory = inventory.ItemsInInventory[i];
                if (Stackable && itemInInventory.Item.ItemID == ItemID && itemInInventory.Count < 99)
                {
                    itemInInventory.Count++;
                    addNewSlot = false;
                }
                inventory.ItemsInInventory[i] = itemInInventory;
            }

            if (inventory.OpenSlot && addNewSlot) 
                inventory.ItemsInInventory.Add(
                    new ItemSlot() {
                    Item = this,
                    Count=1});

        }

        public void RemoveFromInventory(InventoryBase inventory, int IndexOf) // consider having inventory
        {
            ItemSlot updateItem = inventory.ItemsInInventory[IndexOf];
            if (Stackable && updateItem.Count > 1)
            {
                updateItem.Count--;
                inventory.ItemsInInventory[IndexOf] = updateItem;
            }
            else { inventory.ItemsInInventory.RemoveAt(IndexOf); }
        }
    }
    public enum ItemType
    {
        None, General, Weapon, Armor,Crafting_Materials, Blueprint_Recipes,Quest
    }
}