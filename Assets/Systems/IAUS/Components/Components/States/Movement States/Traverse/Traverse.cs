using UnityEngine;
using Unity.Entities;
using System;
namespace IAUS.ECS.Component
{
    [Serializable]
    public struct Traverse : IMovementState
    {

        public uint NumberOfWayPoints { get; set; } 
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public AIStates Name { get { return AIStates.Traverse; } }

        public bool Complete => BufferZone > DistanceToPoint;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; } }
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        [SerializeField] public float DistanceToPoint { get; set; }
        [SerializeField] public float StartingDistance { get; set; }
        [SerializeField] public float BufferZone { get; set; }
        [SerializeField] public float DistanceRatio => (float)DistanceToPoint / (float)StartingDistance != Mathf.Infinity ? Mathf.Clamp01((float)DistanceToPoint / (float)StartingDistance) : 0;
        public bool TravelInOrder { get; set; }

        [SerializeField] public Waypoint CurWaypoint { get; set; }

        public int WaypointIndex { get; set; }
        public float mod { get { return 1.0f - (1.0f / 3.0f); } }


        [HideInInspector] public bool UpdateTravelPoints;
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] public float _resetTime { get; set; }
        [SerializeField] public float _totalScore { get; set; }
    }



    public struct TraverseActionTag : IComponentData
    {
        public bool UpdateWayPoint;

    }
}