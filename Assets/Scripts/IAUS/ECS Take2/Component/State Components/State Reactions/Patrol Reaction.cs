
using Utilities.ReactiveSystem;
using Unity.Entities;
using IAUS.Core;

[assembly: RegisterGenericComponentType(typeof(ReactiveComponentTagSystem<IAUS.ECS2.PatrolActionTag, IAUS.ECS2.Patrol, IAUS.ECS2.Reactions.PatrolTagReactor>.StateComponent))]
[assembly: RegisterGenericComponentType(typeof(ReactiveAIComponentTagSystem<IAUS.ECS2.PatrolActionTag, IAUS.ECS2.Patrol, IAUS.ECS2.WaitTime, IAUS.ECS2.Reactions.PatrolWaitTagReactor>.StateComponent))]


namespace IAUS.ECS2.Reactions
{

    public struct PatrolTagReactor : IComponentReactorTagsForAIStates<PatrolActionTag, Patrol>
    {
        public void ComponentAdded(Entity entity, ref PatrolActionTag newComponent, ref Patrol AIState)
        {
            if (AIState.Status == ActionStatus.Running)
                return;

            AIState.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref Patrol AIState, in PatrolActionTag oldComponent)
        {

            if (!AIState.CanPatrol)
            {
                AIState.Status = ActionStatus.Disabled;
                // patrol.TotalScore = 0.0f;

            }

            switch (AIState.Status)
            {
                case ActionStatus.Failure:
                case ActionStatus.Interrupted:
                    AIState.ResetTime = AIState.ResetTimer / 2.0f;
                    AIState.Status = ActionStatus.CoolDown;
                    break;
                case ActionStatus.Success:
                    AIState.ResetTime = AIState.ResetTimer;
                    AIState.Status = ActionStatus.CoolDown;
                    break;
                case ActionStatus.Idle:
                case ActionStatus.Disabled:
                    AIState.TotalScore = 0.0f;
                    break;
            }
        }

        public void ComponentValueChanged(Entity entity, ref PatrolActionTag newComponent, ref Patrol AIStateCompoment, in PatrolActionTag oldComponent)
        {
        }
        [UpdateInGroup(typeof(IAUS_Reactions))]
  
        public class PatrolReactiveSystem : ReactiveComponentTagSystem<PatrolActionTag, Patrol, PatrolTagReactor>
        {
            protected override PatrolTagReactor CreateComponentReactor()
            {
                return new PatrolTagReactor();
            }


        }
    }


    public struct PatrolWaitTagReactor : IComponentReactorTagsForAIStates<PatrolActionTag, Patrol, WaitTime>
    {
        public void ComponentAdded(Entity entity, ref PatrolActionTag newComponent, ref Patrol AIStateCompoment, ref WaitTime UpdatingComponent)
        {

            //  entityCommandBuffer.RemoveComponent<PatrolActionTag>(nativeThreadIndex, entity);

        }

        public void ComponentRemoved(Entity entity, ref Patrol AIStateCompoment, ref WaitTime UpdatingComponent, in PatrolActionTag oldComponent)
        {
            if (AIStateCompoment.Status == ActionStatus.Success)
            {
                UpdatingComponent.Timer = UpdatingComponent.TimeToWait;
             
            }
        }

        public void ComponentValueChanged(Entity entity, ref PatrolActionTag newComponent, ref Patrol AIStateCompoment, ref WaitTime UpdatingComponent, in PatrolActionTag oldComponent)
        {
        }
        [UpdateInGroup(typeof(IAUS_Reactions))]
   
        public class PatrolWait : ReactiveAIComponentTagSystem<PatrolActionTag, Patrol, WaitTime, PatrolWaitTagReactor>
        {
            protected override PatrolWaitTagReactor CreateComponentReactor()
            {
                return new PatrolWaitTagReactor();
            }
        }
    }
}