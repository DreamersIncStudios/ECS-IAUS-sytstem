using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using Components.MovementSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<TerrorizeAreaTag, TerrorizeAreaState, IAUS.ECS.Systems.Reactive.TerrorizeReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<TerrorizeAreaTag, TerrorizeAreaState, IAUS.ECS.Systems.Reactive.TerrorizeReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<TerrorizeAreaTag, TerrorizeAreaState, IAUS.ECS.Systems.Reactive.TerrorizeReactor>.ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive
{
    public struct TerrorizeReactor : IComponentReactorTagsForAIStates<TerrorizeAreaTag, TerrorizeAreaState>
    {
        public void ComponentAdded(Entity entity, ref TerrorizeAreaTag newComponent, ref TerrorizeAreaState AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
            newComponent.CurSubState = AIStateCompoment.terrorizeSubstate = TerrorizeSubstates.FindTarget;
        }

        public void ComponentRemoved(Entity entity, ref TerrorizeAreaState AIStateCompoment, in TerrorizeAreaTag oldComponent)
        {
            AIStateCompoment.attackThis = new AISenses.Target();
        }

        public void ComponentValueChanged(Entity entity, ref TerrorizeAreaTag newComponent, ref TerrorizeAreaState AIStateCompoment, in TerrorizeAreaTag oldComponent)
        {
           
        }

        public class TerrorizeReactiveSystem : AIReactiveSystemBase<TerrorizeAreaTag, TerrorizeAreaState, TerrorizeReactor>
        {
            protected override TerrorizeReactor CreateComponentReactor()
            {
                return new TerrorizeReactor();
            }
        }
    }


}