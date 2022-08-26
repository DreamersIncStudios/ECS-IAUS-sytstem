using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using System.Collections.Generic;
using DreamersInc.InflunceMapSystem;
using PixelCrushers.LoveHate;
using Unity.Burst;
using IAUS.ECS.Consideration;
using System;
namespace IAUS.ECS.Component
{

    public interface BaseRetreat : IBaseStateScorer
    {
        //Need to add a check to see if escape is possible
        ConsiderationScoringData HealthRatio { get; }
        ConsiderationScoringData ProximityInArea { get; }
        ConsiderationScoringData ThreatInArea { get; }
        int FactionMemberID { get; set; }
        float3 LocationOfHighestThreat { get; }
        float3 LocationOfLowestThreat { get; }
        [SerializeField] public float3 CurPos { get; set; }
        float2 GridValueAtPos { get; }

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