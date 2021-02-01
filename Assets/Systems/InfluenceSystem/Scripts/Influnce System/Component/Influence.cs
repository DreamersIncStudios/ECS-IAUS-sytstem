using Unity.Entities;
using UnityEngine;
namespace InfluenceSystem.Component
{
    [System.Serializable]
    public struct Influence : IComponentData
    {
        public Faction faction;
        public uint InfluenceValue;
        public uint Range;
        public NPCLevel Level;
    }

    public enum NPCLevel{Grunt, Leader, Object}
    public enum Faction
    {
        PlayerParty, Enemy
    }
}