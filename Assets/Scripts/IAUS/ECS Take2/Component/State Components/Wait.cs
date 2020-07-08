
using UnityEngine;
using Unity.Entities;
using Utilities.ReactiveSystem;

[assembly: RegisterGenericComponentType(typeof(ReactiveComponentTagSystem<IAUS.ECS2.WaitActionTag, IAUS.ECS2.WaitTime, IAUS.ECS2.WaitTagReactor>.StateComponent))]

namespace IAUS.ECS2
{
    [GenerateAuthoringComponent]

    public struct WaitTime : BaseStateScorer
    {
        public float TimeToWait;
        public float Timer;
        public bool TimerStarted;
        public ConsiderationData Health;
        public ConsiderationData WaitTimer;
        public float mod { get { return 1.0f - (1.0f / 2.0f); } }

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

    public struct WaitTagReactor : IComponentReactorTagsForAIStates<WaitActionTag, WaitTime>
    {
        public void ComponentAdded(Entity entity, ref WaitActionTag newComponent, ref WaitTime AIState)
        {
            if (AIState.Status == ActionStatus.Running)
                return;

            AIState.Status = ActionStatus.Running;
         
        }

        public void ComponentRemoved(Entity entity, ref WaitTime AIState, in WaitActionTag oldComponent)
        {
        
            switch (AIState.Status)
            {
                case ActionStatus.Running:
                    break;
                case ActionStatus.Interrupted:
                    AIState.ResetTime = AIState.ResetTimer / 2.0f;
                    AIState.Status = ActionStatus.CoolDown;
                    break;
                case ActionStatus.Success:
                    AIState.ResetTime = AIState.ResetTimer;
                    AIState.Status = ActionStatus.CoolDown;
                    break;
                case ActionStatus.Failure:
                    AIState.ResetTime = AIState.ResetTimer / 2.0f;
                    AIState.Status = ActionStatus.CoolDown;
                    break;
                case ActionStatus.Disabled:
                    AIState.TotalScore = 0.0f;
                    break;
            }
        }

        public void ComponentValueChanged(Entity entity, ref WaitActionTag newComponent, ref WaitTime AIStateCompoment, in WaitActionTag oldComponent)
        {
            throw new System.NotImplementedException();
        }

        public class WaitReactiveSystem : ReactiveComponentTagSystem<WaitActionTag, WaitTime, WaitTagReactor>
        {
            protected override WaitTagReactor CreateComponentReactor()
            {
                return new WaitTagReactor();
            }
        }

    }

}
