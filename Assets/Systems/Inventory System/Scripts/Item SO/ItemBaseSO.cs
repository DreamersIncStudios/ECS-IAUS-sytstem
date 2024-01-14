using Unity.Entities;
using UnityEngine;
using Stats;
using Stats.Entities;


namespace Dreamers.InventorySystem.Interfaces
{
    [System.Serializable]
    public abstract class ItemBaseSO : ScriptableObject, IItemBase, IPurchasable
    {
        [SerializeField] private uint itemID;
        public uint ItemID => itemID; // To be implemented with Database system/CSV Editor creator 
        [SerializeField] private string itemName;
        public string ItemName { get { return itemName; } }
        [TextArea(3, 6)]
        [SerializeField] private string _desc;
        public string Description { get { return _desc; } }
        [TextArea(3, 6)]
        public string LongDescription;
        [SerializeField] private Sprite _icon;


        public Sprite Icon { get { return _icon; } }

        [SerializeField] private uint _value;
        public uint Value { get { return _value; } }
        [SerializeField] private ItemType itemType;
        public ItemType ItemType => itemType;
        [SerializeField]  private bool _stackable;
        public bool Stackable { get { return _stackable; } }
        public bool Disposible { get { return !QuestItem; } }
        [SerializeField] bool _questItem;
        public bool QuestItem { get { return _questItem; } }

        [SerializeField]private uint maxStackCount;
        public uint MaxStackCount { get { return maxStackCount; } }
#if UNITY_EDITOR

        public void setItemID(uint ID)
        {

            itemID = ID;
        }
        
#endif

        public virtual void Use(CharacterInventory characterInventory)
        {
            characterInventory.Inventory.RemoveFromInventory(this);
        }

        public virtual void Use(CharacterInventory characterInventory, BaseCharacterComponent player)
        {
            characterInventory.Inventory.RemoveFromInventory(this);
        }

    }
}