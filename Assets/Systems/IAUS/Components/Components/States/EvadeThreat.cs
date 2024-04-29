using AISenses.VisionSystems;
using Components.MovementSystem;
using IAUS.ECS;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public struct EvadeThreat : IBaseStateScorer
    {
        public int Index { get; private set; }
        public AIStates Name => AIStates.Retreat;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; }  }
        public bool InCooldown => Status == ActionStatus.CoolDown;

        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        public float mod { get { return 1.0f - (1.0f / 3.0f); } }
        
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] public float _resetTime { get; set; }
        [SerializeField] public float _totalScore { get; set; }
        public void SetIndex(int index)
        {
            Index = index;
        }

    }
    
    public readonly partial struct RetreatAspect: IAspect
    {
        private readonly RefRW<EvadeThreat> state;
        private readonly VisionAspect visionAspect;
        private readonly RefRO<LocalTransform> transform;
        private readonly RefRW<Movement> move;
    }
}
