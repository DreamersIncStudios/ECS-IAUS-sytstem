using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

namespace IAUS.ECS2.Component
{

    public interface BaseRetreat : IBaseStateScorer
    {
        //Need to add a check to see if escape is possible
        ConsiderationScoringData HealthRatio { get; set; }
        ConsiderationScoringData AlertLevels { get; set; }
        ConsiderationScoringData DistanceToSafe { get; set; }

        Entity WhatIamRunningFrom { get; set; }
        float3 RetreatDirection { get; set; }
        public float distanceToPoint { get; set; }
        public float3 EscapePoint { get; set; }
        public bool HasEscapePoint{ get; }
        bool CanRetreat { get; set; }
        bool Escaped { get; }
        float StartingDistance { get; set; }

    }

    [System.Serializable]
    public struct RetreatCitizen : BaseRetreat {
        public ConsiderationScoringData HealthRatio { get { return healthRatio; } set { healthRatio = value; } }
        [SerializeField] ConsiderationScoringData healthRatio;

        public ConsiderationScoringData AlertLevels { get { return alertLevels; } set { alertLevels = value; } }
        [SerializeField] ConsiderationScoringData alertLevels;
        public ConsiderationScoringData DistanceToSafe { get { return distanceToSafe; } set { distanceToSafe = value; } }
        [SerializeField] ConsiderationScoringData distanceToSafe;

        public Entity WhatIamRunningFrom { get; set; }
        public float3 RetreatDirection { get; set; }
        public bool CanRetreat { get; set; }
        public bool NeedToRetreat;
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
        public float3 EscapePoint { get; set; }

        public bool Escaped => InBufferZone;
        public int EscapeRange;
        public bool HasEscapePoint => !EscapePoint.Equals(float3.zero);

    }

    public struct RetreatEnemyNPC : BaseRetreat {
        public ConsiderationScoringData HealthRatio { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public ConsiderationScoringData AlertLevels { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public ConsiderationScoringData DistanceToSafe { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public Entity WhatIamRunningFrom { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
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
        public bool HasEscapePoint => !EscapePoint.Equals(float3.zero);
        public float StartingDistance { get; set; }


    }

    public struct RetreatPlayerPartyNPC : BaseRetreat {
        public ConsiderationScoringData HealthRatio { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public ConsiderationScoringData AlertLevels { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public ConsiderationScoringData DistanceToSafe { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public Entity WhatIamRunningFrom { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
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
   public bool HasEscapePoint => !EscapePoint.Equals(float3.zero);

        public float StartingDistance { get; set; }


    }

    public struct RetreatActionTag : IComponentData { readonly bool test;
    }

   
}