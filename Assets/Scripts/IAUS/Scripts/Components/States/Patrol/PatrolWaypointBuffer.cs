using Unity.Entities;
using Unity.Mathematics;
using Global.Component;
namespace IAUS.ECS2.Component {

    [System.Serializable]
    [GenerateAuthoringComponent]
    public struct PatrolWaypointBuffer : IBufferElementData
    {
        public Waypoint WayPoint;

    }

    [System.Serializable]
    public struct Waypoint {
        public AITarget Point;
        public float3 Position;
        public float TimeToWaitatWaypoint;
        public float2 InfluenceAtPosition;

        

    }
}