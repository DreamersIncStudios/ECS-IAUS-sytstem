using Stats;
using UnityEngine;
using Dreamers.InventorySystem;
using Dreamers.InventorySystem.Base;

namespace SpawnerSystem.ScriptableObjects {
    public class Enemy : SpawnableSO,  ICharacterStat, ICharacterBase
    {
        [SerializeField] string _name;
        [SerializeField] Gender _gender;
        [SerializeField] CharacterStats _stats;
        [SerializeField] InventoryBase Inventory;
        public string Name { get { return _name; } }
        public Gender gender { get { return _gender; } }


        public CharacterStats Stats { get { return _stats; } }
        // add logic for determine max health and mana 
        // Will Just use base Health for Max health until we add character system to project

        EnemyCharacter EC;
        public override GameObject Spawn(Vector3 Position)
        {
         GameObject spawn =   Instantiate(GO, Position + SpawnOffset, Quaternion.identity);
          EC =spawn.AddComponent<EnemyCharacter>();
            EC.SetAttributeBaseValue(Stats.level, Stats.BaseHealth, Stats.BaseMana, Stats.Str, Stats.vit, Stats.Awr, Stats.Spd, Stats.Skl, Stats.Res, Stats.Con, Stats.Will, Stats.Chars, Stats.Lck);
            spawn.AddComponent<Rigidbody>();
            spawn.AddComponent<CharacterInventory>();


            return spawn;

        }

    }
}