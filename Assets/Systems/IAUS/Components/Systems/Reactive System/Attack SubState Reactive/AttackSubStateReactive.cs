using Components.MovementSystem;
using IAUS.ECS.Component;
using IAUS.ECS.Component.Aspects;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Utilities.ReactiveSystem;


[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<magicmeleeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MagicMeleeTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<magicmeleeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MagicMeleeTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<magicmeleeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MagicMeleeTagReactor>.ManageComponentRemovalJob))]

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<magicAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MagicTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<magicAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MagicTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<magicAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MagicTagReactor>.ManageComponentRemovalJob))]

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<rangeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.RangeTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<rangeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.RangeTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<rangeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.RangeTagReactor>.ManageComponentRemovalJob))]
namespace IAUS.ECS.Systems.Reactive {


    public partial struct MagicMeleeTagReactor : IComponentReactorTagsForAIStates<magicmeleeAttackTag, AttackState>
    {
        public void ComponentAdded(Entity entity, ref magicmeleeAttackTag newComponent, ref AttackState AIStateCompoment)
        {

        }

        public void ComponentRemoved(Entity entity, ref AttackState AIStateCompoment, in magicmeleeAttackTag oldComponent)
        {

        }

        public void ComponentValueChanged(Entity entity, ref magicmeleeAttackTag newComponent, ref AttackState AIStateCompoment, in magicmeleeAttackTag oldComponent)
        {

        }

        public partial class ReactiveSystem : AIReactiveSystemBase<magicmeleeAttackTag, AttackState, MagicMeleeTagReactor>
        {
            protected override MagicMeleeTagReactor CreateComponentReactor()
            {
                return new MagicMeleeTagReactor();
            }

        }
    }
    public partial struct MagicTagReactor : IComponentReactorTagsForAIStates<magicAttackTag, AttackState>
    {
        public void ComponentAdded(Entity entity, ref magicAttackTag newComponent, ref AttackState AIStateCompoment)
        {

        }

        public void ComponentRemoved(Entity entity, ref AttackState AIStateCompoment, in magicAttackTag oldComponent)
        {

        }

        public void ComponentValueChanged(Entity entity, ref magicAttackTag newComponent, ref AttackState AIStateCompoment, in magicAttackTag oldComponent)
        {

        }
    }
    public partial struct RangeTagReactor : IComponentReactorTagsForAIStates<rangeAttackTag, AttackState>
    {
        public void ComponentAdded(Entity entity, ref rangeAttackTag newComponent, ref AttackState AIStateCompoment)
        {

        }

        public void ComponentRemoved(Entity entity, ref AttackState AIStateCompoment, in rangeAttackTag oldComponent)
        {

        }

        public void ComponentValueChanged(Entity entity, ref rangeAttackTag newComponent, ref AttackState AIStateCompoment, in rangeAttackTag oldComponent)
        {

        }
    }

}