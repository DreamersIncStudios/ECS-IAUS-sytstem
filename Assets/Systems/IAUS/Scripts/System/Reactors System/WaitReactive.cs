using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using Components.MovementSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<WaitActionTag, Wait, IAUS.ECS.Systems.Reactive.WaitTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<WaitActionTag, Wait, IAUS.ECS.Systems.Reactive.WaitTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<WaitActionTag, Wait, IAUS.ECS.Systems.Reactive.WaitTagReactor>.ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive
{
    public struct WaitTagReactor : IComponentReactorTagsForAIStates<WaitActionTag, Wait>
    {
        public void ComponentAdded(Entity entity, ref WaitActionTag newComponent, ref Wait AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref Wait AIStateCompoment, in WaitActionTag oldComponent)
        {
            if (AIStateCompoment.Complete)
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
            }
            else
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime * 2;

            }
        }

        public void ComponentValueChanged(Entity entity, ref WaitActionTag newComponent, ref Wait AIStateCompoment, in WaitActionTag oldComponent)
        {
            Debug.Log("Change");
        }

        public class WaitReactiveSystem : AIReactiveSystemBase<WaitActionTag, Wait, WaitTagReactor>
        {
            protected override WaitTagReactor CreateComponentReactor()
            {
                return new WaitTagReactor();
            }
        }
    }

   

}