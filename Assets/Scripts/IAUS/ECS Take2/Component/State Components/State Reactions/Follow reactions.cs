
using Utilities.ReactiveSystem;
using Unity.Entities;
using IAUS.Core;

[assembly: RegisterGenericComponentType(typeof(ReactiveComponentTagSystem<IAUS.ECS2.FollowTargetTag, IAUS.ECS2.FollowCharacter, IAUS.ECS2.Reactions.FollowTagReactor>.StateComponent))]
[assembly: RegisterGenericComponentType(typeof(ReactiveAIComponentTagSystem<IAUS.ECS2.FollowTargetTag,IAUS.ECS2.FollowCharacter,IAUS.ECS2.WaitTime, IAUS.ECS2.Reactions.FollowWaitReactor>.StateComponent))]

namespace IAUS.ECS2.Reactions
{
    public struct FollowTagReactor : IComponentReactorTagsForAIStates<FollowTargetTag, FollowCharacter>
    {
        public void ComponentAdded(Entity entity, ref FollowTargetTag newComponent, ref FollowCharacter AIState)
        {
            if (AIState.Status == ActionStatus.Running)
                return;

            AIState.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref FollowCharacter AIState, in FollowTargetTag oldComponent)
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

        public void ComponentValueChanged(Entity entity, ref FollowTargetTag newComponent, ref FollowCharacter AIStateCompoment, in FollowTargetTag oldComponent)
        {
           
        }
        [UpdateInGroup(typeof(IAUS_Reactions))]

        public class FollowReactiveSystem : ReactiveComponentTagSystem<FollowTargetTag, FollowCharacter, FollowTagReactor>
        {
            protected override FollowTagReactor CreateComponentReactor()
            {
                return new FollowTagReactor();
            }
        }

    }

    public struct FollowWaitReactor : IComponentReactorTagsForAIStates<FollowTargetTag, FollowCharacter, WaitTime>
    {
        public void ComponentAdded(Entity entity, ref FollowTargetTag newComponent, ref FollowCharacter AIStateCompoment, ref WaitTime UpdatingComponent)
        {

        }

        public void ComponentRemoved(Entity entity, ref FollowCharacter AIStateCompoment, ref WaitTime UpdatingComponent, in FollowTargetTag oldComponent)
        {
            if (AIStateCompoment.Status == ActionStatus.Success)
            {
                UpdatingComponent.Timer = UpdatingComponent.TimeToWait;
            }
        }

        public void ComponentValueChanged(Entity entity, ref FollowTargetTag newComponent, ref FollowCharacter AIStateCompoment, ref WaitTime UpdatingComponent, in FollowTargetTag oldComponent)
        {

        }
        [UpdateInGroup(typeof(IAUS_Reactions))]
        public class FollowWait : ReactiveAIComponentTagSystem<FollowTargetTag, FollowCharacter, WaitTime, FollowWaitReactor>
        {
            protected override FollowWaitReactor CreateComponentReactor()
            {
                return new FollowWaitReactor();
            }
        }
    }

}