using DreamersInc.InfluenceMapSystem.Locations;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace IAUS.LevelDesign
{
    public interface IPointOfInterest : IComponentData
    {
        public PointsOfInterestType Type { get; }
        public bool Occupied{get;set;}
    }

    public struct Cover : IPointOfInterest
    {
        public PointsOfInterestType Type=> PointsOfInterestType.Cover;
        public bool Occupied { get; set; }
        public FixedList128Bytes<float3> Positions;
    }

    
}
