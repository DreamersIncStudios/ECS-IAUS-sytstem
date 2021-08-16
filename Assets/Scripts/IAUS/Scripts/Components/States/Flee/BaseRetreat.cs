using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

namespace IAUS.ECS2.Component
{

    public interface BaseRetreat : IBaseStateScorer
    {
        //Need to add a check to see if escape is possible
        ConsiderationScoringData HealthRatio { get; set; }
        ConsiderationScoringData InfluenceInArea { get; set; }
        ConsiderationScoringData DistanceToSafe { get; set; } //TODO Remove Possibly??????? If we are reference the influence in area, once NPC move out of the danger area distance mod should not matter
        float3 LocationOfHighestInflunce { get; set; }

        float3 RetreatDirection { get; set; }
        public float distanceToPoint { get; set; }
        public float3 EscapePoint { get; set; }
        public bool HasEscapePoint{ get; }
        bool CanRetreat { get; set; }
        bool Escaped { get; }
        float StartingDistance { get; set; }
        int EscapeRange { get; set; }
    }

    [System.Serializable]
    public struct RetreatCitizen : BaseRetreat {
        public ConsiderationScoringData HealthRatio { get { return healthRatio; } set { healthRatio = value; } }
        [SerializeField] ConsiderationScoringData healthRatio;

        public ConsiderationScoringData InfluenceInArea { get { return alertLevels; } set { alertLevels = value; } }
        [SerializeField] ConsiderationScoringData alertLevels;
        public ConsiderationScoringData DistanceToSafe { get { return distanceToSafe; } set { distanceToSafe = value; } }
        [SerializeField] ConsiderationScoringData distanceToSafe;

        public float3 LocationOfHighestInflunce { get; set; }
        public float3 RetreatDirection { get; set; }
        [SerializeField] public bool CanRetreat { get; set; }
        [SerializeField] public bool NeedToRetreat;
        public float StartingDistance { get; set;}
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

        public float distanceToPoint { get; set; }
        public float BufferZone;
        public bool InBufferZone => BufferZone > distanceToPoint;
        [SerializeField]public float DistanceRatio => CanRetreat ? Mathf.Clamp01( distanceToPoint / StartingDistance) : 0.0f;
        public float HideTime;
        [SerializeField] public float3 EscapePoint { get; set; }

        public bool Escaped => InBufferZone;
        public int EscapeRange { get { return escapeRange; } set { escapeRange = value; } }
        public int escapeRange;
        [SerializeField] public bool HasEscapePoint => !EscapePoint.Equals(float3.zero);

    }

    public struct RetreatEnemyNPC : BaseRetreat {
        public ConsiderationScoringData HealthRatio { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public ConsiderationScoringData InfluenceInArea { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public ConsiderationScoringData DistanceToSafe { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public float3 LocationOfHighestInflunce { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public float3 RetreatDirection { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool CanRetreat { get; set; }

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
        public bool Escaped { get; }
        public float distanceToPoint { get; set; }
        public float3 EscapePoint { get; set; }
        [SerializeField] public int EscapeRange { get; set; }

        public bool HasEscapePoint => !EscapePoint.Equals(float3.zero);
        public float StartingDistance { get; set; }


    }

    public struct RetreatPlayerPartyNPC : BaseRetreat {
        public ConsiderationScoringData HealthRatio { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public ConsiderationScoringData InfluenceInArea { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public ConsiderationScoringData DistanceToSafe { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public float3 LocationOfHighestInflunce { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public float3 RetreatDirection { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool CanRetreat { get; set; }

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
        public bool Escaped { get; }
        public float distanceToPoint { get; set; }
        public float3 EscapePoint { get; set; }
         public int EscapeRange { get { return escapeRange; }set { escapeRange = value; } }
    public int escapeRange;
        public bool HasEscapePoint => !EscapePoint.Equals(float3.zero);

        public float StartingDistance { get; set; }


    }

    public struct RetreatActionTag : IComponentData { readonly bool test;
    }

   
}