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

    public enum NPCLevel{None,Grunt, Leader, Object}
    public enum Faction
    {
       None, Player, Enemy, Faction2, Faction3, Faction4,//etc etc 
    }
}