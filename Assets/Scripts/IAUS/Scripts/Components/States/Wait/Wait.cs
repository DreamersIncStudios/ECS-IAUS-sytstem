using UnityEngine;
using Unity.Entities;
namespace IAUS.ECS2.Component {
    [System.Serializable]
    [GenerateAuthoringComponent]
    public struct Wait : IBaseStateScorer
    {
        public ConsiderationScoringData TimeLeft;
        public ConsiderationScoringData HealthRatio;
        public bool Complete => Timer <= 0.0f;
        public float Timer;
        public float StartTime;
        public float TimePercent => Timer / StartTime;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; }  }
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;

        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        public float mod { get { return 1.0f - (1.0f / 2.0f); } }
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;
    }

    public struct WaitActionTag : IComponentData
    {
        public bool tester;
    }


}
