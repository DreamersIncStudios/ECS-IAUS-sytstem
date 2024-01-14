using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Utilities.ReactiveSystem;
using DreamersInc.ComboSystem;
using MotionSystem.Components;
using Stats.Entities;
using Unity.Jobs;


[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<AnimationSpeedMod, CharControllerE, DreamersInc.CharacterControllerSys.AnimSpeedReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<AnimationSpeedMod, CharControllerE, DreamersInc.CharacterControllerSys.AnimSpeedReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<AnimationSpeedMod, CharControllerE, DreamersInc.CharacterControllerSys.AnimSpeedReactor>.ManageComponentRemovalJob))]
namespace DreamersInc.CharacterControllerSys
{
    public struct AnimSpeedReactor : IComponentReactorTagsForAIStates<AnimationSpeedMod, CharControllerE>
    {
        public void ComponentAdded(Entity entity, ref AnimationSpeedMod newComponent, ref CharControllerE AIStateCompoment)
        {
        }

        public void ComponentRemoved(Entity entity, ref CharControllerE AIStateCompoment, in AnimationSpeedMod oldComponent)
        {
        }

        public void ComponentValueChanged(Entity entity, ref AnimationSpeedMod newComponent, ref CharControllerE AIStateCompoment, in AnimationSpeedMod oldComponent)
        {
        }

        partial class AnimSpeedSystem : AIReactiveSystemBase<AnimationSpeedMod, CharControllerE, AnimSpeedReactor>
        {



            protected override AnimSpeedReactor CreateComponentReactor()
            {
                return new AnimSpeedReactor();
            }

            /*          */
        }
    }
    partial class stupid : SystemBase
    {

        private EntityQuery _componentAddedQuery;
        private EntityQuery _componentRemovedQuery;
        protected override void OnCreate()
        {

            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AnimationSpeedMod)), ComponentType.ReadWrite(typeof(CharControllerE)), ComponentType.ReadWrite(typeof(Animator)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<AnimationSpeedMod, CharControllerE, DreamersInc.CharacterControllerSys.AnimSpeedReactor>.StateComponent)) }
            });
            _componentRemovedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Animator)), ComponentType.ReadWrite(typeof(AIReactiveSystemBase<AnimationSpeedMod, CharControllerE, DreamersInc.CharacterControllerSys.AnimSpeedReactor>.StateComponent)), ComponentType.ReadWrite(typeof(CharControllerE)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AnimationSpeedMod)) }
            });
        }

        protected override void OnUpdate()
        {

            Entities.WithoutBurst().ForEach((Animator anim, ref AnimationSpeedMod tag) =>
            {
                if (anim.GetFloat("AnimSpeed") == 1.0)
                {
                    anim.SetFloat("AnimSpeed", .15f);
                }

            }).Run();


            Entities.WithoutBurst().WithNone<AnimationSpeedMod>().ForEach((Animator anim) =>
            {
                if (anim.GetFloat("AnimSpeed") == .15f)
                {
                    anim.SetFloat("AnimSpeed", 1.0f);
                }
            }).Run();
        }
    }
}