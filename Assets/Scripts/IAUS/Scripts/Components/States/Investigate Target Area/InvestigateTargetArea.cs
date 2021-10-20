using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using System;
using IAUS.ECS.Consideration;
namespace IAUS.ECS.Component
{
    [Serializable]
    [GenerateAuthoringComponent]
    public struct InvestigateTargetArea : IBaseStateScorer
    {
        public int refIndex { get; set; }

        public ConsiderationScoringData HealthRatio;
        public ConsiderationScoringData InfluenceInArea;
        public ConsiderationScoringData DistanceToSafe;

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
    }
}