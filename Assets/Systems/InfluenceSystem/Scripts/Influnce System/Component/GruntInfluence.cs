using Unity.Entities;
using UnityEngine;
namespace InfluenceSystem.Component
{
    [GenerateAuthoringComponent]
    public struct GruntInfluence : IComponentData
    {
        public Faction faction;
        public uint InfluenceValue;
        public uint Range;
    }
}
