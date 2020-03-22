using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;


namespace IAUS.ECS2
{
    [GenerateAuthoringComponent]
    public struct InvestigateArea : BaseStateScorer
    {
        #region Considerations
        public ConsiderationData Health;
        public ConsiderationData DistanceToTarget;
        public ConsiderationData DetectionLevel;
        #endregion


        #region BaseState
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _resetTimer;
        [SerializeField] float _resetTime;

        [SerializeField] float _totalScore;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }


        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float ResetTimer { get { return _resetTimer; } set { _resetTimer = value; } }
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        #endregion
    }

    public struct InvestigateAreaTag : IComponentData { }



}