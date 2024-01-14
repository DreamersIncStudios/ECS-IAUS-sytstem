using Unity.Entities;
using UnityEngine;
using Stats;

namespace Dreamers.InventorySystem.Interfaces
{

    public interface IItemBase
    {
        uint ItemID { get; }
        string ItemName { get; }
        string Description {get;}
        Sprite Icon { get; }
        ItemType ItemType { get; }
        bool Stackable { get; }
        bool Disposible { get; }
        bool QuestItem { get; }

        void Use(CharacterInventory characterInventory);

    }
    public enum ItemType
    {
        None, General, Weapon, Armor,Crafting_Materials, Blueprint_Recipes,Quest
    }
}