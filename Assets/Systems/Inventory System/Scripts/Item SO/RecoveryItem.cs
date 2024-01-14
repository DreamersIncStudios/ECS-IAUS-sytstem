using System.Collections;
using System.Collections.Generic;
using Dreamers.InventorySystem.Interfaces;
using Stats;
using Stats.Entities;
using UnityEngine;


namespace Dreamers.InventorySystem
{


    public class RecoveryItem : ItemBaseSO
    {
        [SerializeReference]public List<IItemAction> RecoveryItems;

        public override void Use(CharacterInventory characterInventory, BaseCharacterComponent player)
        {
            base.Use(characterInventory,player);
            foreach (var item in RecoveryItems)
            {
                item.Use(player);
            }

        }
    }
}