using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

using System.Diagnostics;

namespace Utilities.ReactiveSystem
{
    public interface IComponentReactorAIState<AICOMPONENT>
    {
        bool Added { get; set; }
        void ComponentAdded(Entity entity, ref AICOMPONENT newComponent, ref EntityCommandBuffer ECB);
        void ComponentRemoved(Entity entity, in AICOMPONENT oldComponent);

    }
    public abstract class ReactiveComponentAISystem< AICOMPONENT, AICOMPONENT_REACTOR> : SystemBase
    where AICOMPONENT : unmanaged, IComponentData
    where AICOMPONENT_REACTOR : struct, IComponentReactorAIState<AICOMPONENT>
    {
        /// <summary>
        /// Struct implmenting IComponentReactor<COMPONENT> that implements the behavior when COMPONENT is added, removed or changed value.
        /// </summary>
        private AICOMPONENT_REACTOR _reactor;
        /// <summary>
        /// Query to detect the addition of COMPONENT to an entity.
        /// </summary>
        private EntityQuery _componentAddedQuery;
        /// <summary>
        /// Query to detect the removal of COMPONENT from an entity.
        /// </summary>
        private EntityQuery _componentRemovedQuery;
        /// <summary>
        /// Query to gateher all entity that need to check for change in value.
        /// </summary>
        private EntityQuery _componentValueChangedQuery;
        /// <summary>
        /// EnityCommandBufferSystem used to add and remove the StateComponent.
        /// </summary>
        private EntityCommandBufferSystem _entityCommandBufferSystem;
        /// <summary>
        /// The state component for this reactive system.
        /// It contains a copy of the COMPONENT data.
        /// </summary>
        public struct StateComponent : ISystemStateComponentData
        {
            public AICOMPONENT Value;
        }
        protected override void OnCreate()
        {
            base.OnCreate();
            _reactor = CreateComponentReactor();
            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AICOMPONENT)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(StateComponent)) }
            });
            _componentRemovedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(StateComponent)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AICOMPONENT)) }
            });
         
            _entityCommandBufferSystem = GetCommandBufferSystem();
        }
        /// <summary>
        /// Create the reactor struct that implements the behavior when COMPONENT is added, removed or changed value.
        /// </summary>
        /// <returns>COMPONENT_REACTOR</returns>
        protected abstract AICOMPONENT_REACTOR CreateComponentReactor();
        /// <summary>
        /// Get the EntityCommandBufferSystem buffer system to use to add and remove the StateComponent.
        /// </summary>
        /// <returns>EntityCommandBufferSystem</returns>
        protected EntityCommandBufferSystem GetCommandBufferSystem()
        {
            return World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        void Additions() 
        {
             EntityCommandBuffer entityCommandBuffer = GetCommandBufferSystem().CreateCommandBuffer();
             AICOMPONENT_REACTOR Reactor;
            NativeArray<AICOMPONENT> aiComponents = _componentAddedQuery.ToComponentDataArray<AICOMPONENT>(Allocator.Temp);
            NativeArray<Entity> entities = _componentAddedQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; ++i)
            {
                AICOMPONENT AIcomponent = aiComponents[i];
                Entity entity = entities[i];
                _reactor.ComponentAdded(entity, ref AIcomponent, ref entityCommandBuffer);
                aiComponents[i] = AIcomponent;
                entityCommandBuffer.AddComponent<StateComponent>(entities[i]);
                entityCommandBuffer.SetComponent( entities[i], new StateComponent() { Value = AIcomponent });
            }

                entities.Dispose();
            aiComponents.Dispose();

        }

        protected override void OnUpdate()
        {
           

        }
    }
}