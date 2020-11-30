
using Utilities.ReactiveSystem;
using Unity.Entities;
using IAUS.Core;
using System.Numerics;

[assembly: RegisterGenericComponentType(typeof(ReactiveComponentTagSystem<IAUS.ECS2.WaitActionTag, IAUS.ECS2.WaitTime, IAUS.ECS2.Reactions.WaitTagReactor>.StateComponent))]

[assembly: RegisterGenericComponentType(typeof(ReactiveAIComponentTagSystem<IAUS.ECS2.WaitActionTag, IAUS.ECS2.WaitTime, IAUS.ECS2.Patrol, IAUS.ECS2.Reactions.WaitPatrolReactor>.StateComponent))]
[assembly: RegisterGenericComponentType(typeof(ReactiveAIComponentTagSystem<IAUS.ECS2.WaitActionTag, IAUS.ECS2.WaitTime, IAUS.ECS2.FollowCharacter, IAUS.ECS2.Reactions.WaitFollowReactor>.StateComponent))]


namespace IAUS.ECS2.Reactions
{

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

        public void ComponentValueChanged(Entity entity, ref WaitActionTag newComponent, ref WaitTime AIStateCompoment, in WaitActionTag oldComponent)
        {
           
        }
        [UpdateInGroup(typeof(IAUS_Reactions))]

        public class WaitReactiveSystem : ReactiveComponentTagSystem<WaitActionTag, WaitTime, WaitTagReactor>
        {
            protected override WaitTagReactor CreateComponentReactor()
            {
                return new WaitTagReactor();
            }
        }

    }


    public struct WaitPatrolReactor : IComponentReactorTagsForAIStates<WaitActionTag, WaitTime, Patrol>
    {
        public void ComponentAdded(Entity entity, ref WaitActionTag newComponent, ref WaitTime AIStateCompoment, ref Patrol UpdatingComponent)
        {
       
        }

        public void ComponentRemoved(Entity entity, ref WaitTime AIStateCompoment, ref Patrol UpdatingComponent, in WaitActionTag oldComponent)
        {
            //AIStateCompoment.WaitTimer
            if (AIStateCompoment.Status == ActionStatus.Success)
            {
                UpdatingComponent.index++;
                if (UpdatingComponent.index >= UpdatingComponent.MaxNumWayPoint)
                    UpdatingComponent.index = 0;
                UpdatingComponent.UpdatePosition = true;

            }
        }

        public void ComponentValueChanged(Entity entity, ref WaitActionTag newComponent, ref WaitTime AIStateCompoment, ref Patrol UpdatingComponent, in WaitActionTag oldComponent)
        {
          
        }
        [UpdateInGroup(typeof(IAUS_Reactions))]

        public class ReactionSystem : ReactiveAIComponentTagSystem<WaitActionTag, WaitTime, Patrol, WaitPatrolReactor>
        {
            protected override WaitPatrolReactor CreateComponentReactor()
            {
                return new WaitPatrolReactor();
            }
        }
    }

    public struct WaitFollowReactor : IComponentReactorTagsForAIStates<WaitActionTag, WaitTime, FollowCharacter>
    {
        public void ComponentAdded(Entity entity, ref WaitActionTag newComponent, ref WaitTime AIStateCompoment, ref FollowCharacter UpdatingComponent)
        {
            // throw new System.NotImplementedException();
        }

        public void ComponentRemoved(Entity entity, ref WaitTime AIStateCompoment, ref FollowCharacter UpdatingComponent, in WaitActionTag oldComponent)
        {
            //throw new System.NotImplementedException();
        }

        public void ComponentValueChanged(Entity entity, ref WaitActionTag newComponent, ref WaitTime AIStateCompoment, ref FollowCharacter UpdatingComponent, in WaitActionTag oldComponent)
        {
            //    throw new System.NotImplementedException();
        }
        public class WaitFollowSystem : ReactiveAIComponentTagSystem<WaitActionTag, WaitTime, FollowCharacter, WaitFollowReactor>
        {
            protected override WaitFollowReactor CreateComponentReactor()
            {
                return new WaitFollowReactor();
            }
        }
    }
}