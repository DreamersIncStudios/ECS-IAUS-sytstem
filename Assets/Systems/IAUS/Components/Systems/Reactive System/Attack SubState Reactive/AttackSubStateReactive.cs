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


[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<MagicMeleeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MagicMeleeTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<MagicMeleeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MagicMeleeTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<MagicMeleeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MagicMeleeTagReactor>.ManageComponentRemovalJob))]

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<MagicAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MagicTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<MagicAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MagicTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<MagicAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MagicTagReactor>.ManageComponentRemovalJob))]

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<RangeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.RangeTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<RangeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.RangeTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<RangeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.RangeTagReactor>.ManageComponentRemovalJob))]
namespace IAUS.ECS.Systems.Reactive {


    public partial struct MagicMeleeTagReactor : IComponentReactorTagsForAIStates<MagicMeleeAttackTag, AttackState>
    {
        public void ComponentAdded(Entity entity, ref MagicMeleeAttackTag newComponent, ref AttackState AIStateCompoment)
        {

        }

        public void ComponentRemoved(Entity entity, ref AttackState AIStateCompoment, in MagicMeleeAttackTag oldComponent)
        {

        }

        public void ComponentValueChanged(Entity entity, ref MagicMeleeAttackTag newComponent, ref AttackState AIStateCompoment, in MagicMeleeAttackTag oldComponent)
        {

        }

        public partial class ReactiveSystem : AIReactiveSystemBase<MagicMeleeAttackTag, AttackState, MagicMeleeTagReactor>
        {
            protected override MagicMeleeTagReactor CreateComponentReactor()
            {
                return new MagicMeleeTagReactor();
            }

        }
    }
    public partial struct MagicTagReactor : IComponentReactorTagsForAIStates<MagicAttackTag, AttackState>
    {
        public void ComponentAdded(Entity entity, ref MagicAttackTag newComponent, ref AttackState AIStateCompoment)
        {

        }

        public void ComponentRemoved(Entity entity, ref AttackState AIStateCompoment, in MagicAttackTag oldComponent)
        {

        }

        public void ComponentValueChanged(Entity entity, ref MagicAttackTag newComponent, ref AttackState AIStateCompoment, in MagicAttackTag oldComponent)
        {

        }
    }
    public partial struct RangeTagReactor : IComponentReactorTagsForAIStates<RangeAttackTag, AttackState>
    {
        public void ComponentAdded(Entity entity, ref RangeAttackTag newComponent, ref AttackState AIStateCompoment)
        {

        }

        public void ComponentRemoved(Entity entity, ref AttackState AIStateCompoment, in RangeAttackTag oldComponent)
        {

        }

        public void ComponentValueChanged(Entity entity, ref RangeAttackTag newComponent, ref AttackState AIStateCompoment, in RangeAttackTag oldComponent)
        {

        }
    }

}