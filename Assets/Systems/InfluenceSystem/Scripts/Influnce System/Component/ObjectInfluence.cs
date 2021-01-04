using Unity.Entities;
using UnityEngine;
namespace InfluenceSystem.Component
{
    [GenerateAuthoringComponent]
    public struct ObjectInfluence : IComponentData
    {
        public Faction faction;
        public uint InfluenceValue;
        public uint Range;
    }
}