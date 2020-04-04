using Unity.Entities;
using UnityEngine;
namespace InfluenceMap.Factions
{
    [GenerateAuthoringComponent]
    public struct Attackable : IComponentData
    {
        public FactionTypes Faction;
        public ObjectType Type;
        public FactionAggression AggressionLevels; // This might need to be moved to factionTag

        // system to get this points Break out to subclass


    }
    public enum ObjectType {
        Structure,
        Creature,
        Humaniod,
    }
    public enum FactionTypes {
        Human,
        Angel,
        Daemon,
        Robot,
    }
    [System.Serializable]
    public struct FactionAggression
    {
        [Range(-10,10)]
        public float Humans;
        [Range(-10, 10)]
        public float Daemons;
        [Range(-10, 10)]
        public float Angels;
    }
}
