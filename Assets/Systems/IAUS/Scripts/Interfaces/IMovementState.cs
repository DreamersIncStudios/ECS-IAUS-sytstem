using IAUS.Components.States;

namespace IAUS.Interfaces
{
    public interface IMovementState: IAIState
    {
        public uint NumberOfWayPoints { get; }
        public uint CurrentWaypointIndex { get; set; }
        public float DistanceToPoint { get; set; }
        public float StartingDistanceToPoint { get; set; }
        public float StoppingDistance { get; set; }
        public bool Complete { get; }
        public bool TravelInOrder { get; set; }
    }
}