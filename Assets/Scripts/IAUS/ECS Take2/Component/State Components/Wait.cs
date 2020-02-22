
using UnityEngine;
using Unity.Entities;

namespace IAUS.ECS2
{
    [GenerateAuthoringComponent]

    public struct WaitTime : BaseStateScorer
    {
        public float TimeToWait;
        public float Timer;
        public bool TimerStarted;
        public ConsiderationData Health;
        public ConsiderationData DistanceToTarget;
        public ConsiderationData WaitTimer;

        [SerializeField] float _totalScore;
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _resetTimer;
        [SerializeField] public float _resetTime;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }

        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float ResetTimer { get { return _resetTimer; } set { _resetTimer = value; } }
        public float ResetTime { get { return _resetTime; }  set { _resetTime = value; } }
    }
}
