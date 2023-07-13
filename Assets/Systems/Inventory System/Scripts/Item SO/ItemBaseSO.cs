using Unity.Entities;
using UnityEngine;
using Stats;
using Stats.Entities;

namespace Dreamers.InventorySystem.Interfaces
{
    [System.Serializable]
    public abstract class ItemBaseSO : ScriptableObject, IItemBase,IPurchasable
    {
        [SerializeField] private uint _itemID;
        public uint ItemID { get { return _itemID; }  } // To be implemented with Database system/CSV Editor creator 
        [SerializeField] private string _itemName;
        public string ItemName { get { return _itemName; } }
        [TextArea(3,6)]
        [SerializeField] private string _desc;
        public string Description { get { return _desc; } }
        [SerializeField] private Sprite _icon;
        public Sprite Icon { get { return _icon; } }

        [SerializeField] private uint _value;
        public uint Value { get { return _value; } }
        [SerializeField] private ItemType _type;
        public ItemType Type { get { return _type; } }
        [SerializeField]  private bool _stackable;
        public bool Stackable { get { return _stackable; } }
        //[SerializeField] bool _disposible;
        public bool Disposible { get { return !QuestItem; } }
        [SerializeField] bool _questItem;
        public bool QuestItem { get { return _questItem; } }

        [SerializeField]private uint maxStackCount;
        public uint MaxStackCount { get { return maxStackCount; } }
#if UNITY_EDITOR

        public void setItemID(uint ID)
        {

            _itemID = ID;
        }
#endif

        public virtual void Use(CharacterInventory characterInventory)
        {
            characterInventory.Inventory.RemoveFromInventory(this);
        }

        public abstract void Use(CharacterInventory characterInventory, BaseCharacter player);


    }
}