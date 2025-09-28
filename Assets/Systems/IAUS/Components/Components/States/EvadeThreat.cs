using AISenses;
using AISenses.VisionSystems;
using Components.MovementSystem;
using DreamersIncStudio.FactionSystem;
using IAUS.ECS.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public struct EvadeThreat : IBaseStateScorer
    {
        public EvadePlan Plan;
        public float3 EvadeTargetLocation;
        public int Index { get; private set; }
        public AIStates Name => AIStates.Retreat;

        public float TotalScore
        {
            get => _totalScore;
            set => _totalScore = value;
        }

        public ActionStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public float CoolDownTime
        {
            get { return _coolDownTime; }
        }

        public bool InCooldown => Status == ActionStatus.CoolDown;

        public float ResetTime
        {
            get { return _resetTime; }
            set { _resetTime = value; }
        }

        public float mod
        {
            get { return 1.0f - (1.0f / 3.0f); }
        }

        private ActionStatus _status;
        private float _coolDownTime;

        public EvadeThreat(float cool)
        {
            _coolDownTime = cool;
            EvadeTargetLocation = CheckInfluencePos = float3.zero;
            InfluenceAtPoint = int2.zero;
            _status = ActionStatus.Idle;
            _totalScore = 0.0f;
            _resetTime = 0.0f;
            Index = 0;
            Plan = EvadePlan.None;
        }

        private float _resetTime { get; set; }
        private float _totalScore { get; set; }
        public float3 CheckInfluencePos;
        public int2 InfluenceAtPoint;

        public void SetIndex(int index)
        {
            Index = index;
        }
    }


    public enum EvadePlan
    {
        None,
        MoveToCover,
        MoveToLocation
    }

    public readonly partial struct RetreatAspect : IAspect
    {
        private readonly RefRW<EvadeThreat> state;
        private readonly RefRO<LocalToWorld> transform;
        private readonly RefRW<Movement> move;
        private readonly DynamicBuffer<Enemies> targets;

        public void DeterminePlan()
        {
            if (targets.IsEmpty) return;

            //Find the closest Enemy
            var maxDist = float.MaxValue;
            var indexOfTarget = -1;
            for (var i = 0; i < targets.Length; i++)
            {
                if (targets[i].Target.Affinity is Affinity.Love or Affinity.Positive) continue;
                if (!(targets[i].Target.DistanceTo < maxDist)) continue;
                maxDist = targets[i].Target.DistanceTo;
                indexOfTarget = i;
            }

            var enemyLocation = (indexOfTarget != -1) ? targets[indexOfTarget].Target.LastKnownPosition : float3.zero;

            //Find Cover or Location outside of Enemy Range
            if (indexOfTarget == -1)
            {
                state.ValueRW.Plan = EvadePlan.None;
                return;
            }

            /*
            state.ValueRW.Plan = mapCover.ValueRO.CoverPositions.Equals(float3x4.zero)
                ? EvadePlan.MoveToLocation
                : EvadePlan.MoveToCover;*/
        }

        public void ExecutePlan()
        {
        }
    }

    [UpdateInGroup(typeof(IAUSUpdateGroup))]
    public partial struct TestingEvade : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            new Jobtest().Schedule();
        }

        partial struct Jobtest : IJobEntity
        {
            void Execute(Entity entity, RetreatAspect evade)
            {
                evade.DeterminePlan();
            }
        }
    }
}