using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;
namespace InfluenceSystem.Component
{
    [GenerateAuthoringComponent]
    public struct LeaderInfluence : IComponentData
    {
        public Faction faction;
        public uint InfluenceValue;
        public uint Range;

    }

 
    public enum Faction { 
        PlayerParty, Enemy
    }

}
