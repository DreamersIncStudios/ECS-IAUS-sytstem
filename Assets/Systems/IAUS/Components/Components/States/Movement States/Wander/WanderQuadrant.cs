using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.ECS.Consideration;
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

        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public int Index;

        [SerializeField] public bool Complete { get { return BufferZone > distanceToPoint; } }
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; } }
        public bool InCooldown => Status == ActionStatus.CoolDown;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

        public bool TravelInOrder { get; set; }
        public uint NumberOfWayPoints { get { return 1; } set { } }

        [SerializeField] public float DistanceRatio => (float)distanceToPoint / (float)StartingDistance != Mathf.Infinity ? Mathf.Clamp01((float)distanceToPoint / (float)StartingDistance) : 0;


        public AIStates name => AIStates.WanderQuadrant;

        public int WaypointIndex { get { return 1; } set { } }
        public float3 TravelPosition;
        [SerializeField] public Waypoint CurWaypoint { get; set; }

         public float distanceToPoint { get; set; }
        [SerializeField] public float StartingDistance { get; set; }
        [SerializeField] public float BufferZone { get; set; }

        public float mod { get { return 1.0f - (1.0f / 3.0f); } }

        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] public float _resetTime { get; set; }
        [SerializeField] public float _totalScore { get; set; }
      [SerializeField]  public bool AttackTarget { get; set; }

        public float3 SpawnPosition;
        public int Hashkey
        {
            get { return NPCQuadrantSystem.GetPositionHashMapKey((int3)SpawnPosition); }
        }
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
        readonly VisionAspect VisionAspect;
        BlobAssetReference<AIStateBlobAsset> reference => SetupAIStateBlob.reference;
        float TravelInFiveSec
        {
            get
            {
                return 5 * 10; // TODO change to stat dependent statInfo.ValueRO.Speed * 5;   
            }
        }

        bool targetInRange
        {
            get
            {
                if (VisionAspect.TargetInRange(out _, out float dist)) {
                    return dist < TravelInFiveSec;
                }
                else { return false; }
            }
        }
        float distanceToPoint
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
                    throw new ArgumentOutOfRangeException(nameof(wander), $"Please check Creature list and Consideration Data to make sure {wander.ValueRO.name} state is implements");

                }
                float totalScore;
                wander.ValueRW.distanceToPoint = distanceToPoint;
                if (VisionAspect.TargetInRange(out _, out float dist))
                {
                    totalScore = reference.Value.Array[wander.ValueRO.Index].DistanceToPlaceOfInterest.Output(wander.ValueRO.DistanceRatio) * reference.Value.Array[wander.ValueRO.Index].Health.Output(statInfo.ValueRO.HealthRatio) *
                        reference.Value.Array[wander.ValueRO.Index].DistanceToTarget.Output(Mathf.Clamp01(dist/TravelInFiveSec));
                }
                else
                {
                    totalScore = reference.Value.Array[wander.ValueRO.Index].DistanceToPlaceOfInterest.Output(wander.ValueRO.DistanceRatio) * reference.Value.Array[wander.ValueRO.Index].Health.Output(statInfo.ValueRO.HealthRatio)*
                           reference.Value.Array[wander.ValueRO.Index].DistanceToTarget.Output(Mathf.Clamp01(0));
                }
                wander.ValueRW.TotalScore = !targetInRange &&wander.ValueRO.Status != ActionStatus.CoolDown && !wander.ValueRO.AttackTarget ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * wander.ValueRO.mod) * totalScore) : 0.0f;

                totalScore = wander.ValueRW.TotalScore;
                return totalScore;
            }
        }
        public ActionStatus Status { get => wander.ValueRO.Status; }
    }
}