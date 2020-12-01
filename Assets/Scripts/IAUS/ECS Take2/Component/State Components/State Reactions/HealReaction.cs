
using Utilities.ReactiveSystem;
using Unity.Entities;
using IAUS.Core;

[assembly: RegisterGenericComponentType(typeof(ReactiveComponentTagSystem<IAUS.ECS2.HealSelfActionTag, IAUS.ECS2.HealSelfViaItem, IAUS.ECS2.Reactions.HealSelfTagReactor>.StateComponent))]

namespace IAUS.ECS2.Reactions
{
    public struct HealSelfTagReactor : IComponentReactorTagsForAIStates<HealSelfActionTag, HealSelfViaItem>
    {
        public void ComponentAdded(Entity entity, ref HealSelfActionTag newComponent, ref HealSelfViaItem AIState)
        {
            if (AIState.Status == ActionStatus.Running)
                return;

            AIState.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref HealSelfViaItem AIState, in HealSelfActionTag oldComponent)
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

        public void ComponentValueChanged(Entity entity, ref HealSelfActionTag newComponent, ref HealSelfViaItem AIState, in HealSelfActionTag oldComponent)
        {


        }
        [UpdateInGroup(typeof(IAUS_Reactions))]
        public class HealReactiveSystem : ReactiveComponentTagSystem<HealSelfActionTag, HealSelfViaItem, HealSelfTagReactor>
        {
            protected override HealSelfTagReactor CreateComponentReactor()
            {
                return new HealSelfTagReactor();
            }
        }


    }


}