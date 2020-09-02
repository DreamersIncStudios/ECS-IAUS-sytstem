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
            set { if (value < 10 && value > -10)
                    _humans = value;
                if (value > 10)
                    _humans = 10;
                if (value < -10)
                    _humans = -10;
            }
        }

        [Range(-10, 10)]
        [SerializeField] private int _daemons;

        public int Daemons
        {
            get { return _daemons; }
            set
            {
                if (value < 10 && value > -10)
                    _daemons = value;
                if (value > 10)
                    _daemons = 10;
                if (value < -10)
                    _daemons = -10;
            }
        }
        [Range(-10, 10)]
        [SerializeField] private int _angels;

        public int Angels
        {
            get { return _angels; }
            set
            {
                if (value < 10 && value > -10)
                    _angels = value;
                if (value > 10)
                    _angels = 10;
                if (value < -10)
                    _angels = -10;
            }
        }
        [Range(-10, 10)]
        [SerializeField] private int _mechas;
        public int Mechas
        {
            get { return _mechas; }
            set
            {
                if (value < 10 && value > -10)
                    _mechas = value;
                if (value > 10)
                    _mechas = 10;
                if (value < -10)
                    _mechas = -10;
            }
        }
    }
}
