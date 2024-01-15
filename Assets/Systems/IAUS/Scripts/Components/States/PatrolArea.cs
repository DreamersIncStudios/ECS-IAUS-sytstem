using IAUS.BlobAssets;
using IAUS.Interfaces;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace IAUS.Components.States
{
    public struct PatrolArea:IMovementState
    {
        public AIStates Name => AIStates.Patrol;
        [SerializeField]public uint NumberOfWayPoints => (uint)Waypoints.Capacity;

        public uint CurrentWaypointIndex { get; set; }
        public bool Complete => StoppingDistance > DistanceToPoint;

        public float DistanceToPoint { get; set; }
        public float StartingDistanceToPoint { get; set; }
        public float DistanceRatio => (float)DistanceToPoint / (float)StartingDistanceToPoint != Mathf.Infinity ?  Mathf.Clamp01((float)DistanceToPoint / (float)StartingDistanceToPoint ): 0;

        public float StoppingDistance { get; set; }
        public bool TravelInOrder { get; set; }
        public float TotalScore { get; }
        public ActionStatus Status { get; set; }
        public float CoolDownTime { get; }
        public bool InCooldown { get; }
        public float ResetTime { get; set; }
        public float mod => 1.0f - (1.0f / 4.0f);
        public FixedList64Bytes<uint> Waypoints;
        public BlobAssetReference<WaypointData> WaypointBlobRef;
        public int Index { get; private set; }

        public void SetIndex(int index) {
            Index= index;
        }
    }
        /// <summary>
     /// Point for ai to move to 
     /// </summary> 
    public struct Waypoint {
        public float3 Position;
        public float TimeToWaitatWaypoint;
    }
}