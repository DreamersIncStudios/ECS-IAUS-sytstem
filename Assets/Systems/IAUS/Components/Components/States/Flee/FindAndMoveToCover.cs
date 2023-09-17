using DreamersInc.InflunceMapSystem;
using IAUS.ECS.Consideration;
using IAUS.ECS.StateBlobSystem;
using PixelCrushers.LoveHate;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace IAUS.ECS.Component
{
    public struct FindAndMoveToCover : BaseRetreat
    {
        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public int Index;
        public AIStates name { get { return AIStates.RetreatToLocation; } }
        public ConsiderationScoringData HealthRatio => stateRef.Value.Array[Index].Health;
        public ConsiderationScoringData ProximityInArea => stateRef.Value.Array[Index].FriendlyInfluence;
        public ConsiderationScoringData ThreatInArea => stateRef.Value.Array[Index].EnemyInfluence;
        public ConsiderationScoringData DistanceFromThreat => stateRef.Value.Array[Index].DistanceToTargetEnemy;
        public int FactionMemberID { get; set; }

        public float ThreatThreshold;
        public float CrowdMin;
        public float RetreatRange; // TODO set based off speed stat and balance

        [BurstDiscard]
        public float3 LocationOfHighestThreat
        {
            get
            {
                InfluenceGridMaster.Instance.grid.GetGridObject(CurPos).GetHighestThreatCell(LoveHate.factionDatabase.GetFaction(FactionMemberID), true, out int x, out int y);
                return InfluenceGridMaster.Instance.grid.GetWorldPosition(x, y);
            }
        }

        [BurstDiscard]
        public float3 LocationOfLowestThreat
        {
            get
            {
                InfluenceGridMaster.Instance.grid.GetGridObject(CurPos).GetLowestThreatCell(LoveHate.factionDatabase.GetFaction(FactionMemberID), true, out int x, out int y);
                return InfluenceGridMaster.Instance.grid.GetWorldPosition(x, y);
            }
        }

        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; } }
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        public float mod { get { return 1.0f - (1.0f / 3.0f); } }

        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] public float _resetTime { get; set; }
        [SerializeField] public float _totalScore { get; set; }

        public float HideTime;

        public float3 CurPos { get; set; }
        [BurstDiscard]
        public float2 InfluenceValueAtPos
        {
            get
            {
                return InfluenceGridMaster.Instance.grid.GetGridObject(CurPos).GetValueNormalized(LoveHate.factionDatabase.GetFaction(FactionMemberID));
            }
        }

    }

}