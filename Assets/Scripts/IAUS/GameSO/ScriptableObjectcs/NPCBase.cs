using UnityEngine;
using SpawnerSystem.ScriptableObjects;
using InfluenceMap.Factions;
using SpawnerSystem;

namespace ProjectRebirth.Bestiary
{
    public abstract class NPCBase : SpawnableSO, ICharacterStat, ICharacterBase
    {
        [SerializeField] string _name;
        [SerializeField] uint _level;
        [SerializeField] int _baseHealth;
        [SerializeField] int _baseMana;
        [SerializeField] Gender _gender;


        public string Name { get { return _name; } }
        public Gender gender { get { return _gender; } }
        public uint Level { get { return _level; } }
        public int BaseHealth { get { return _baseHealth; } }
        public int BaseMana { get { return _baseMana; } }


        public abstract void Spawn(Vector3 pos);



    }
}