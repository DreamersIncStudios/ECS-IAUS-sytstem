using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dreamers.InventorySystem.Base
{
    [Serializable]
    public class StoreBase
    {
        public string StoreName;
        [SerializeField]
        public InventoryBase StoreInventory;
         public CharacterInventory CharacterInventory;
        [Range(.5f, 1.0f)]
        public float Sell;
        [Range(.5f, 1.0f)]
        public float Buy;
        //public StoreBase(CharacterInventory CharacterInventoryInput) {

        //    CharacterInventory = CharacterInventoryInput;
        //}
        bool CanPurchase(ItemSlot Item, out int Saleprice) {
            Saleprice = Mathf.RoundToInt( Item.Item.Value * Sell);
            return CharacterInventory.Gold > Saleprice * Sell;
                }
        bool CanPurchaseX(ItemSlot Item, uint Multiplier, out int Saleprice)
        {
            Saleprice = Mathf.RoundToInt(Item.Item.Value * Sell* Multiplier);
            return CharacterInventory.Gold > Saleprice * Sell;
        }
        public void BuyItemFrom(ItemSlot ItemToBuy) {
            //int salePrice;
            if (CanPurchase(ItemToBuy, out int salePrice)
                && CharacterInventory.Inventory.OpenSlots(ItemToBuy)) 
            {
                CharacterInventory.Gold -= salePrice;
                ItemToBuy.Item.AddToInventory(CharacterInventory.Inventory);
                Debug.Log("Bought one " + ItemToBuy.Item.ItemName);
            }
        }
        public void BuyXItemsFrom(ItemSlot ItemToBuy, uint Multiplier)
        {
            //int salePrice;
            if (CanPurchaseX(ItemToBuy, Multiplier, out int salePrice)
                && CharacterInventory.Inventory.OpenSlots(ItemToBuy))
            {
                CharacterInventory.Gold -= salePrice;
                for (int i = 0; i < Multiplier; i++)
                {
                    ItemToBuy.Item.AddToInventory(CharacterInventory.Inventory);
                }
                    Debug.Log("Bought " +Multiplier.ToString()+" "+ ItemToBuy.Item.ItemName);

            }
        }
        public void SellItemTo(ItemSlot ItemToSell, int IndexOf) {
            if (ItemToSell.Item.Type != ItemType.Quest) {
                CharacterInventory.Gold += Mathf.RoundToInt(ItemToSell.Item.Value * (1 * Sell));
                ItemToSell.Item.RemoveFromInventory(CharacterInventory.Inventory, IndexOf);
            }
            else { Debug.Log("Can't Sell Item "+ ItemToSell.Item.ItemName); }


        }
        public void SellxItemsTo(ItemSlot ItemToSell, int IndexOf, uint Multiplier )
        {
            if (ItemToSell.Item.Stackable && ItemToSell.Count >= Multiplier && ItemToSell.Item.Type != ItemType.Quest)
            {
                CharacterInventory.Gold += Mathf.RoundToInt(ItemToSell.Item.Value * (1* Sell));
                for (int i = 0; i < Multiplier; i++)
                {
                    ItemToSell.Item.RemoveFromInventory(CharacterInventory.Inventory, IndexOf);
                }
            }
            else { Debug.Log("Can't Sell Item " + ItemToSell.Item.ItemName); }

        }
    }
}