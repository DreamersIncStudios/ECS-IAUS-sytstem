using Unity.Entities;
using Unity.Mathematics;
using Global.Component;
using System;
namespace IAUS.ECS.Component {

    [Serializable]
    public struct TravelWaypointBuffer : IBufferElementData
    {
        public Waypoint WayPoint;

    }

    [Serializable]
    public struct Waypoint {
        public AITarget Point;
        public float3 Position;
        public float TimeToWaitatWaypoint;
        public float2 InfluenceAtPosition;
        public bool Avoid;

        

    }
}