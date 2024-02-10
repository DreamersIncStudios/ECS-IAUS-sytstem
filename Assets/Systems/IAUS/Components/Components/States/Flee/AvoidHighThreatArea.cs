using DreamersInc.InflunceMapSystem;
using IAUS.ECS.Consideration;
using IAUS.ECS.StateBlobSystem;
using PixelCrushers.LoveHate;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace IAUS.ECS.Component
{
    [Serializable]
    public struct RetreatCitizen : BaseRetreat
    {
        public BlobAssetReference<AIStateBlobAsset> stateRef;
 
        public AIStates Name { get { return AIStates.RetreatToLocation; } }
        public ConsiderationScoringData HealthRatio => stateRef.Value.Array[Index].Health;
        public ConsiderationScoringData ProximityInArea => stateRef.Value.Array[Index].FriendlyInfluence;
        public ConsiderationScoringData ThreatInArea => stateRef.Value.Array[Index].EnemyInfluence;
        public ConsiderationScoringData DistanceFromThreat => stateRef.Value.Array[Index].DistanceToTargetEnemy;
        public float ThreatThreshold;
        public float CrowdMin;
        public float RetreatRange; // TODO set based off speed stat and balance
        public int FactionMemberID { get; set; }

        public float3 LocationOfHighestThreat { get;set;  }

        public float3 LocationOfLowestThreat { get; set; }

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
        [BurstDiscard]
        public float2 InfluenceValueAtPos { get; set; }
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }

    }
}