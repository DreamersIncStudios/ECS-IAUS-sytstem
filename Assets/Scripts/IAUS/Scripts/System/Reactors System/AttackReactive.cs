using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using IAUS.ECS2.Component;
using Unity.Entities;
using Unity.Burst;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<AttackTargetActionTag,MeleeAttackTarget, IAUS.ECS2.Systems.Reactive.MeleeAttackReactor>.StateComponent))]

namespace IAUS.ECS2.Systems.Reactive
{
    public struct MeleeAttackReactor : IComponentReactorTagsForAIStates<AttackTargetActionTag, MeleeAttackTarget>
    {
        public void ComponentAdded(Entity entity, ref AttackTargetActionTag newComponent, ref MeleeAttackTarget AIStateCompoment)
        {
            Debug.Log("Attacked Target");
            AIStateCompoment.Timer = 30; 
        }

        public void ComponentRemoved(Entity entity, ref MeleeAttackTarget AIStateCompoment, in AttackTargetActionTag oldComponent)
        {

                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
        }

        public void ComponentValueChanged(Entity entity, ref AttackTargetActionTag newComponent, ref MeleeAttackTarget AIStateCompoment, in AttackTargetActionTag oldComponent)
        {
        
        }

        public class MeleeAttackReactiveSystem : AIReactiveSystemBase<AttackTargetActionTag, MeleeAttackTarget, MeleeAttackReactor>
        {
            protected override MeleeAttackReactor CreateComponentReactor()
            {
                return new MeleeAttackReactor();
            }
        }
    }

    public class AttackSystem : SystemBase
    {
        EntityQuery MeleeAttackers;
        protected override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }
    }

}