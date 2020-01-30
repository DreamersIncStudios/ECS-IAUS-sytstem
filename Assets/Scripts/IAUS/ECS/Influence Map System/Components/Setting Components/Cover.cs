﻿using Unity.Entities;

namespace InfluenceMap {
    [GenerateAuthoringComponent]
    [System.Serializable]
    public struct Cover : IComponentData {
        public DestructionType Destruction;
        public bool Destroyed;
        public int level;
        public int CurHealth;
        public int MaxHealth;
        
        public float HPratio { get { return (float)CurHealth / MaxHealth; } }

        public int Range;
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
