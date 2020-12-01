
using UnityEngine;
using Unity.Entities;
using Utilities.ReactiveSystem;


namespace IAUS.ECS2
{
    [GenerateAuthoringComponent]

    public struct WaitTime : BaseStateScorer
    {
        public float TimeToWait;
        public float Timer;
        public bool TimerRunning => Timer > 0.0f;
        public ConsiderationData Health;
        public ConsiderationData WaitTimer;
        public float RatioForScore { get { return Timer / TimeToWait; } }
        public float mod { get { return 1.0f - (1.0f / 2.0f); } }
        public bool RemoveTag => Status == ActionStatus.Success || Status == ActionStatus.Interrupted ||
            Status == ActionStatus.Disabled;
        [SerializeField] float _totalScore;
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _resetTimer;
        [SerializeField] public float _resetTime;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }

        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float ResetTimer { get { return _resetTimer; } set { _resetTimer = value; } }
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
    }
    public struct WaitActionTag : IComponentData {
        bool test;
    }



}
