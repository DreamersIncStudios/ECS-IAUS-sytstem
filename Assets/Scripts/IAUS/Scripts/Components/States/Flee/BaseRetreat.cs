using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using System.Collections.Generic;
using DreamersInc.InflunceMapSystem;
using DreamersInc.FactionSystem;
using Unity.Burst;
using IAUS.ECS.Consideration;
using System;
namespace IAUS.ECS.Component
{

    public interface BaseRetreat : IBaseStateScorer
    {
        //Need to add a check to see if escape is possible
        ConsiderationScoringData HealthRatio { get; set; }
        ConsiderationScoringData ProximityInArea { get; set; }
        ConsiderationScoringData ThreatInArea { get; set; }
        int FactionMemberID { get; set; }
        float3 LocationOfHighestThreat { get;  }
        float3 LocationOfLowestThreat { get;  }
        float3 CurPos { get; set; }
        float2 GridValueAtPos { get; }

    }

    [Serializable]
    public struct RetreatCitizen : BaseRetreat {
        public int refIndex { get; set; }

        public ConsiderationScoringData HealthRatio { get { return healthRatio; } set { healthRatio = value; } }
        [SerializeField] ConsiderationScoringData healthRatio;

        public ConsiderationScoringData ProximityInArea { get { return influenceInArea; } set { influenceInArea = value; } }
        [SerializeField] ConsiderationScoringData influenceInArea;

        public ConsiderationScoringData ThreatInArea { get { return threatInArea; } set { threatInArea = value; } }
        [SerializeField] ConsiderationScoringData threatInArea;

        public int FactionMemberID
        { get { return faction; } set { faction = value; } }
        [SerializeField] int faction;
        [BurstDiscard]
        public float3 LocationOfHighestThreat
        {
            get
            {
                InfluenceGridMaster.Instance.grid.GetGridObject(CurPos).GetHighestThreatCell(FactionManager.Database.GetFaction(FactionMemberID),true, out int x, out int y);
                return InfluenceGridMaster.Instance.grid.GetWorldPosition(x, y);
            }
        }

        [BurstDiscard] public float3 LocationOfLowestThreat {
            get   
            {
                InfluenceGridMaster.Instance.grid.GetGridObject(CurPos).GetLowestThreatCell(FactionManager.Database.GetFaction(FactionMemberID), true, out int x, out int y);
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
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;

        public float HideTime;

        public float3 CurPos { get; set; }
        [BurstDiscard] public float2 GridValueAtPos
        { get {
                return InfluenceGridMaster.Instance.grid.GetGridObject(CurPos).GetValueNormalized(FactionManager.Database.GetFaction(FactionMemberID));
                    }  }

    }

    //TODO Implement over Retreat States

    //public struct RetreatEnemyNPC : BaseRetreat {
    //    public ConsiderationScoringData HealthRatio { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public ConsiderationScoringData InfluenceInArea { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public ConsiderationScoringData DistanceToSafe { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public float3 LocationOfHighestInflunce { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public float3 RetreatDirection { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public bool CanRetreat { get; set; }

    //    public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
    //    public ActionStatus Status { get { return _status; } set { _status = value; } }
    //    public float CoolDownTime { get { return _coolDownTime; } }
    //    public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
    //    public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
    //    public float mod { get { return 1.0f - (1.0f / 3.0f); } }
    //    [SerializeField] public ActionStatus _status;
    //    [SerializeField] public float _coolDownTime;
    //    [SerializeField] float _resetTime;
    //    [SerializeField] float _totalScore;
    //    public bool Escaped { get; }
    //    public float distanceToPoint { get; set; }
    //    public float3 EscapePoint { get; set; }
    //    [SerializeField] public int EscapeRange { get; set; }

    //    public bool HasEscapePoint => !EscapePoint.Equals(float3.zero);
    //    public float StartingDistance { get; set; }
    //    public int ThreatInArea { get; set; }
    //    public int FriendllyProximityInArea { get; set; }

    //}

    //public struct RetreatPlayerPartyNPC : BaseRetreat {
    //    public ConsiderationScoringData HealthRatio { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public ConsiderationScoringData InfluenceInArea { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public ConsiderationScoringData DistanceToSafe { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public float3 LocationOfHighestInflunce { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public float3 RetreatDirection { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public bool CanRetreat { get; set; }

    //    public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
    //    public ActionStatus Status { get { return _status; } set { _status = value; } }
    //    public float CoolDownTime { get { return _coolDownTime; } }
    //    public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
    //    public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
    //    public float mod { get { return 1.0f - (1.0f / 3.0f); } }
    //    [SerializeField] public ActionStatus _status;
    //    [SerializeField] public float _coolDownTime;
    //    [SerializeField] float _resetTime;
    //    [SerializeField] float _totalScore;
    //    public bool Escaped { get; }
    //    public float distanceToPoint { get; set; }
    //    public float3 EscapePoint { get; set; }
    //     public int EscapeRange { get { return escapeRange; }set { escapeRange = value; } }
    //public int escapeRange;
    //    public bool HasEscapePoint => !EscapePoint.Equals(float3.zero);

    //    public float StartingDistance { get; set; }
    //    public int ThreatInArea { get; set; }
    //    public int FriendllyProximityInArea { get; set; }

    //}

    public struct RetreatActionTag : IComponentData { readonly bool test;
    }

   
}