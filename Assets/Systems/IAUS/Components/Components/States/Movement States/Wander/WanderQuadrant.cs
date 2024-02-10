using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using DreamersInc.QuadrantSystems;


namespace IAUS.ECS.Component
{
    public struct WanderQuadrant : IMovementState
    {
        
        public int Index { get; private set; }
        public bool Complete { get { return BufferZone > DistanceToPoint; } }
        public float TotalScore { get { return totalScore; } set { totalScore = value; } }
        public ActionStatus Status {get; set; }
        public float CoolDownTime { get { return coolDownTime; } }
        public bool InCooldown => Status == ActionStatus.CoolDown;
        public float ResetTime { get { return resetTime; } set { resetTime = value; } }

        public bool TravelInOrder { get; set; }
        public uint NumberOfWayPoints { get { return 1; } set { } }

        public float DistanceRatio  => (float)DistanceToPoint / (float)StartingDistance != Mathf.Infinity ? Mathf.Clamp01((float)DistanceToPoint / (float)StartingDistance) : 0;


        public AIStates Name => AIStates.WanderQuadrant;

        public int WaypointIndex { get => 1;
            set { } }
        public float3 TravelPosition;
        public Waypoint CurWaypoint { get; set; }

        [SerializeField]public float DistanceToPoint { get; set; }
        [SerializeField] public float StartingDistance { get; set; }
        [SerializeField] public float BufferZone { get; set; }

        public float mod => 1.0f - (1.0f / 3.0f);
        
        float coolDownTime;
        float resetTime { get; set; }
        float totalScore { get; set; }
      [SerializeField]  public bool AttackTarget { get; set; }

        public float3 SpawnPosition;
        public int HashKey => NPCQuadrantSystem.GetPositionHashMapKey((int3)SpawnPosition);
        public bool WanderNeighborQuadrants;

        public WanderQuadrant(float3 spawnPosition, float coolDownTime, float bufferZone, bool wanderNeighborQuadrants) : this()
        {
            SpawnPosition = spawnPosition;
            BufferZone = bufferZone;
            this.coolDownTime = coolDownTime;
            WanderNeighborQuadrants = wanderNeighborQuadrants;
        }

        public void SetIndex(int index)
        {
            Index = index;
        }

    }
    public struct WanderActionTag : IComponentData
    {
        public float WaitTime;

    }

 
}