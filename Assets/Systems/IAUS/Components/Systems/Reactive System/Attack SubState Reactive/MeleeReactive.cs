using AISenses.VisionSystems.Combat;
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

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<meleeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MeleeTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<meleeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MeleeTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<meleeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MeleeTagReactor>.ManageComponentRemovalJob))]

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<meleeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MeleeTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<meleeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MeleeTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<meleeAttackTag, AttackState, IAUS.ECS.Systems.Reactive.MeleeTagReactor>.ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive
{
    public partial struct MeleeTagReactor : IComponentReactorTagsForAIStates<meleeAttackTag, AttackState>
    {
        public void ComponentAdded(Entity entity, ref meleeAttackTag newComponent, ref AttackState AIStateCompoment)
        {

        }

        public void ComponentRemoved(Entity entity, ref AttackState AIStateCompoment, in meleeAttackTag oldComponent)
        {

        }

        public void ComponentValueChanged(Entity entity, ref meleeAttackTag newComponent, ref AttackState AIStateCompoment, in meleeAttackTag oldComponent)
        {

        }

        public partial class ReactiveSystem : AIReactiveSystemBase<meleeAttackTag, AttackState, MeleeTagReactor>
        {
            protected override MeleeTagReactor CreateComponentReactor()
            {
                return new MeleeTagReactor();
            }

        }
    }

    public partial class MoveMeleeTagReactor : SystemBase
    {
        EntityQuery componentAdded;

        protected override void OnCreate()
        {
            base.OnCreate();
            componentAdded = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadWrite(typeof(AttackState)), ComponentType.ReadWrite(typeof(AttackTarget)),
                ComponentType.ReadWrite(typeof(meleeAttackTag))},
                Absent = new ComponentType[] { ComponentType.ReadWrite(typeof(AIReactiveSystemBase<meleeAttackTag, AttackState, MeleeTagReactor>.StateComponent)) }
            });
        }
        protected override void OnUpdate()
        {
            new SetMoveTarget().Schedule(componentAdded);
        }

        partial struct SetMoveTarget : IJobEntity
        {
            public void Execute(ref AttackTarget target, ref Movement move)
            {
                move.SetLocation(target.AttackTargetLocation);

            }
        }
    }
}
