using Unity.Entities;
using UnityEngine;
namespace InfluenceMap {
    [System.Serializable]
    [GenerateAuthoringComponent]
    public struct Cover : IComponentData {
        public DestructionType Destruction;
        public bool Destroyed;
        public int level;
        [HideInInspector]public int CurHealth;
        public int MaxHealth; // Think about calculating or making this a scriptableobject 
        
        public float HPratio { get { return (float)CurHealth / MaxHealth; } }

        public int DamageRange;
        public int Damage;


    }

    public enum DestructionType {
        None,
        Explosive, 
        Breakable,
        Burnable,  // possibly add a second enum for secondary effects
        Corrovise
    }
}

