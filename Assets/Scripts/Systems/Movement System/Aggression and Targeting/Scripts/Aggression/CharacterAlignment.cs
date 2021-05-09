using System;
using Unity.Entities;
using UnityEngine;
namespace CharacterAlignmentSystem
{

   [Serializable]
   [GenerateAuthoringComponent]
    public struct CharacterAlignment : IComponentData 
    {
        public Alignment Faction;
        public ObjectType Type;
        public FactionAggression AggressionLevels; // This might need to be moved to factionTag

        // system to get this points Break out to subclass


    }
    public enum ObjectType {
        Structure,
        Creature,
        Humaniod,
        Mecha_Creature,
        Mecha_Humaniod,
        Mecha_Bot
    }
    public enum Alignment {
        Human,
        Angel,
        Daemon,
        Mecha,
    }
    [Serializable]
    public struct FactionAggression
    {
        [Range(-10,10)]
        [SerializeField]private int _humans;
        [SerializeField] public int Humans { get { return _humans; }
            set { _humans = Mathf.Clamp(value, -10, 10); }
        }

        [Range(-10, 10)]
        [SerializeField] private int _daemons;

        public int Daemons
        {
            get { return _daemons; }
            set { _daemons = Mathf.Clamp(value, -10, 10); }
        }
        [Range(-10, 10)]
        [SerializeField] private int _angels;

        public int Angels
        {
            get { return _angels; }
            set
            {
                _angels = Mathf.Clamp(value, -10, 10);
            }
        }
        [Range(-10, 10)]
        [SerializeField] private int _mechas;
        public int Mechas
        {
            get { return _mechas; }
            set { _mechas= Mathf.Clamp(value, -10, 10); }
        
        }
    }
}
