using UnityEngine;
using IAUS.ECS.StateBlobSystem;
using Unity.Mathematics;
using Unity.Entities;
using Stats.Entities;
using Unity.Transforms;
using DreamersInc.QuadrantSystems;
using System;
using AISenses.VisionSystems;

namespace IAUS.ECS.Component
{
    public struct WanderQuadrant : IMovementState
    {
        
        public int Index;
      
        /// <summary>
        /// Utility score for Attackable target in Ranges
        /// </summary>
      //  public ConsiderationScoringData TargetInRange => stateRef.Value.Array[Index].DistanceToTarget;
      //  public ConsiderationScoringData Influence => stateRef.Value.Array[Index].EnemyInfluence;

        [SerializeField] public bool Complete { get { return BufferZone > distanceToPoint; } }
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; } }
        public bool InCooldown => Status == ActionStatus.CoolDown;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

        public bool TravelInOrder { get; set; }
        public uint NumberOfWayPoints { get { return 1; } set { } }

        [SerializeField] public float DistanceRatio => !float.IsPositiveInfinity((float)distanceToPoint / (float)StartingDistance) ? Mathf.Clamp01((float)distanceToPoint / (float)StartingDistance) : 0;


        public AIStates Name => AIStates.WanderQuadrant;

        public int WaypointIndex { get => 1;
            set { } }
        public float3 TravelPosition;
        [SerializeField] public Waypoint CurWaypoint { get; set; }

         public float distanceToPoint { get; set; }
        [SerializeField] public float StartingDistance { get; set; }
        [SerializeField] public float BufferZone { get; set; }

        public float mod => 1.0f - (1.0f / 3.0f);

        public ActionStatus _status;
        public float _coolDownTime;
        public float _resetTime { get; set; }
        public float _totalScore { get; set; }
      [SerializeField]  public bool AttackTarget { get; set; }

        public float3 SpawnPosition;
        public int HashKey => NPCQuadrantSystem.GetPositionHashMapKey((int3)SpawnPosition);
        public bool WanderNeighborQuadrants;
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
        float DistanceToPoint
        {
            get
            {
                float dist = new();
                if (wander.ValueRO.Complete)
                {
                    dist = 0.0f;
                }
                dist = Vector3.Distance(wander.ValueRO.TravelPosition, transform.ValueRO.Position);
                return dist;
            }
        }
        public float Score
        {
            get
            {
                if (wander.ValueRO.Index == -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(wander), $"Please check Creature list and Consideration Data to make sure {wander.ValueRO.Name} state is implements");
                }

                var asset = GetAsset(wander.ValueRO.Index);
                wander.ValueRW.distanceToPoint = DistanceToPoint;
                var distToEnemy = vision.GetClosestEnemy().DistanceTo;
                var totalScore = asset.DistanceToTargetLocation.Output(wander.ValueRO.DistanceRatio)* asset.Health.Output(statInfo.ValueRO.HealthRatio)*
                                   asset.DistanceToTargetEnemy.Output(Mathf.Clamp01(distToEnemy/50.0f)); //TODO Add Back Later * escape.ValueRO.TargetInRange.Output(attackRatio); ;
                wander.ValueRW.TotalScore = wander.ValueRO.Status != ActionStatus.CoolDown && !wander.ValueRO.AttackTarget ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * wander.ValueRO.mod) * totalScore) : 0.0f;

                totalScore = wander.ValueRW.TotalScore;
                return totalScore;
            }
        }
        public ActionStatus Status { get => wander.ValueRO.Status; }
    }
}