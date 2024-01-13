using UnityEngine;
using IAUS.ECS.StateBlobSystem;
using Unity.Mathematics;
using Unity.Entities;
using Stats.Entities;
using Unity.Transforms;
using DreamersInc.QuadrantSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using AISenses.VisionSystems;
using Components.MovementSystem;


namespace IAUS.ECS.Component
{
    public struct WanderQuadrant : IMovementState
    {
        
        public int Index { get; private set; }
        public bool Complete { get { return BufferZone > DistanceToPoint; } }
        public float TotalScore { get { return totalScore; } set { totalScore = value; } }
        public ActionStatus Status { get { return status; } set { status = value; } }
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

        ActionStatus status;
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
    public readonly partial struct WanderQuadrantAspect : IAspect
    {
        readonly RefRO<LocalTransform> transform;
        readonly RefRO<AIStat> statInfo;
        readonly RefRW<WanderQuadrant> wander;
        private readonly VisionAspect vision;
        private readonly RefRO<IAUSBrain> brain;


        private StateAsset GetAsset(int index)
        {
            return brain.ValueRO.State.Value.Array[index];
        }

        private float DistanceToPoint => Vector3.Distance(wander.ValueRO.TravelPosition, transform.ValueRO.Position) < wander.ValueRO.BufferZone
            ? 0 : Vector3.Distance(wander.ValueRO.TravelPosition, transform.ValueRO.Position);

        public float Score
        {
            get
            {
                if (wander.ValueRO.Index == -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(wander), $"Please check Creature list and Consideration Data to make sure {wander.ValueRO.Name} state is implements");
                }
                if (vision.TargetInReactRange)
                {
                    return 0.0f;
                }
                if (wander.ValueRO.Status == ActionStatus.Idle && wander.ValueRO.SpawnPosition.Equals(wander.ValueRO.TravelPosition))
                {
                    wander.ValueRW.SpawnPosition.x += 35;
                    wander.ValueRW.SpawnPosition.z += 45;
                    wander.ValueRW.StartingDistance = DistanceToPoint;
                }
             
                var asset = GetAsset(wander.ValueRO.Index);
                wander.ValueRW.DistanceToPoint = DistanceToPoint;


                var distToEnemy = vision.GetClosestEnemy().Entity != Entity.Null
                    ? vision.GetClosestEnemy().DistanceTo
                    : 50;
                var totalScore = Mathf.Clamp01(asset.DistanceToTargetLocation.Output(wander.ValueRO.DistanceRatio)* asset.Health.Output(statInfo.ValueRO.HealthRatio)*
                                 asset.DistanceToTargetEnemy.Output(Mathf.Clamp01(distToEnemy/50.0f))); //TODO Add Back Later * escape.ValueRO.TargetInRange.Output(attackRatio); ;
                wander.ValueRW.TotalScore = wander.ValueRO.Status != ActionStatus.CoolDown && !wander.ValueRO.AttackTarget ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * wander.ValueRO.mod) * totalScore) : 0.0f;

                totalScore = wander.ValueRW.TotalScore;
                return totalScore;
            }
        }
        public ActionStatus Status => wander.ValueRO.Status;
    }

 
}