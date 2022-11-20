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
            throw new System.NotImplementedException();
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

    [UpdateBefore(typeof(TerrorizeReactor.TerrorizeReactiveSystem))]
    public partial class UpdateTerrorizeState : SystemBase
    {
        private EntityQuery _componentAddedQuery;
        private EntityQuery _terrorize;
        EntityCommandBufferSystem _entityCommandBufferSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(TerrorizeAreaState)), ComponentType.ReadWrite(typeof(TerrorizeAreaStateTag)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer))
                , ComponentType.ReadOnly(typeof(LocalToWorld))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<TerrorizeAreaStateTag, TerrorizeAreaState, TerrorizeReactor>.StateComponent)) }
            });
            _terrorize = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(TerrorizeAreaState)), ComponentType.ReadWrite(typeof(TerrorizeAreaStateTag)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer))
                , ComponentType.ReadOnly(typeof(LocalToWorld)),
                ComponentType.ReadOnly(typeof(AIReactiveSystemBase<TerrorizeAreaStateTag, TerrorizeAreaState, TerrorizeReactor>.StateComponent)) }
            });

            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }
        protected override void OnUpdate()
        { 
        
        }
    }
}