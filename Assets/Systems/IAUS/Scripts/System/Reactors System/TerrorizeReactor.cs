using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using Components.MovementSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<TerrorizeAreaStateTag, TerrorizeAreaState, IAUS.ECS.Systems.Reactive.TerrorizeReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<TerrorizeAreaStateTag, TerrorizeAreaState, IAUS.ECS.Systems.Reactive.TerrorizeReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<TerrorizeAreaStateTag, TerrorizeAreaState, IAUS.ECS.Systems.Reactive.TerrorizeReactor>.ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive
{
    public struct TerrorizeReactor : IComponentReactorTagsForAIStates<TerrorizeAreaStateTag, TerrorizeAreaState>
    {
        public void ComponentAdded(Entity entity, ref TerrorizeAreaStateTag newComponent, ref TerrorizeAreaState AIStateCompoment)
        {
            newComponent.CurSubState = AIStateCompoment.terrorizeSubstate = TerrorizeSubstates.FindTarget;
        }

        public void ComponentRemoved(Entity entity, ref TerrorizeAreaState AIStateCompoment, in TerrorizeAreaStateTag oldComponent)
        {
            throw new System.NotImplementedException();
        }

        public void ComponentValueChanged(Entity entity, ref TerrorizeAreaStateTag newComponent, ref TerrorizeAreaState AIStateCompoment, in TerrorizeAreaStateTag oldComponent)
        {
            throw new System.NotImplementedException();
        }

        public class TerrorizeReactiveSystem : AIReactiveSystemBase<TerrorizeAreaStateTag, TerrorizeAreaState, TerrorizeReactor>
        {
            protected override TerrorizeReactor CreateComponentReactor()
            {
                return new TerrorizeReactor();
            }
        }
    }


}