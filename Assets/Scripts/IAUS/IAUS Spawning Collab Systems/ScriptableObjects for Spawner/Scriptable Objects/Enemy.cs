using Stats;
using UnityEngine;
using Dreamers.InventorySystem;
using Dreamers.InventorySystem.Base;
using UnityEngine.UIElements.Experimental;

namespace SpawnerSystem.ScriptableObjects {
    public class Enemy : SpawnableSO,  ICharacterStat, ICharacterBase
    {
        [SerializeField] private string _name;
        [SerializeField] private Gender _gender;
        [SerializeField] private CharacterStats _stats;
        [SerializeField] private InventoryBase Inventory;

        public string Name { get { return _name; } }
        public Gender gender { get { return _gender; } }


        public CharacterStats Stats { get { return _stats; } }
        // add logic for determine max health and mana 
        // Will Just use base Health for Max health until we add character system to project

        private EnemyCharacter EC;
        public override GameObject Spawn(Vector3 Position)
        {
         GameObject spawn =   Instantiate(GO, Position + SpawnOffset, Quaternion.identity);
          EC =spawn.AddComponent<EnemyCharacter>();
            EC.SetAttributeBaseValue(Stats.level, Stats.BaseHealth, Stats.BaseMana, Stats.Str, Stats.vit, Stats.Awr, Stats.Spd, Stats.Skl, Stats.Res, Stats.Con, Stats.Will, Stats.Chars, Stats.Lck);
            spawn.AddComponent<Rigidbody>();
            CharacterInventory InventoryChar = spawn.AddComponent<CharacterInventory>();
            InventoryChar.Inventory.MaxInventorySize = 20;

            foreach (ItemSlot itemSlot in Inventory.ItemsInInventory)
            {
                if (itemSlot.Item.Stackable)
                {
                    for (int i = 0; i < itemSlot.Count; i++)
                    {
                        itemSlot.Item.AddToInventory(InventoryChar.Inventory);
                    }
                }
                else
                {
                    itemSlot.Item.AddToInventory(InventoryChar.Inventory);
                }

            }

            return spawn;

        }

    }
}