using Unity.Entities;

namespace IAUS.ECS2.Component {

    [System.Serializable]
    [GenerateAuthoringComponent]
    public struct PatrolWaypointBuffer : IBufferElementData
    {
        public Waypoint WayPoint;

        public static implicit operator Waypoint(PatrolWaypointBuffer e) { return e; }
        public static implicit operator PatrolWaypointBuffer(Waypoint e) { return new PatrolWaypointBuffer { WayPoint = e }; }

    }

    [System.Serializable]
    public struct Waypoint {
        public AITarget Point;
        public float TimeToWaitatWaypoint;

    }
}