using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;


namespace Utilities.ReactiveSystem
{
    public interface IComponentReactor<COMPONENT> 
    {
        void ComponentAdded(ref COMPONENT newComponent);
        void ComponentRemoved(in COMPONENT oldComponent);
        void ComponentValueChanged(ref COMPONENT newComponent, in COMPONENT oldComponent);
    }

    public abstract class ReactiveComponentSystem<COMPONENT, COMPONENT_REACTOR> : SystemBase 
        where COMPONENT: unmanaged, IComponentData
        where COMPONENT_REACTOR: struct, IComponentReactor<COMPONENT>
    {
        private COMPONENT_REACTOR _reactor;
        private EntityQuery _componentAddedQuery;
        private EntityQuery _componentRemovedQuery;
        private EntityQuery _componentValueChangedQuery;
        private EntityCommandBufferSystem _entityCommandBufferSystem;

        public struct StateComponent : ISystemStateComponentData
        {
            public COMPONENT Value;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            _reactor = CreateComponentReactor();
            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(COMPONENT)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(StateComponent)) }
            });
            _componentRemovedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(StateComponent)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(COMPONENT)) }
            });
            _componentValueChangedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(COMPONENT)), ComponentType.ReadWrite(typeof(StateComponent)) }
            });
            _entityCommandBufferSystem = GetCommandBufferSystem();
        }

        protected abstract COMPONENT_REACTOR CreateComponentReactor();

        protected EntityCommandBufferSystem GetCommandBufferSystem() 
        {
            return World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        /// <summary>
        /// This system call the COMPONENT_REACTOR.ComponentAdded method on all enttiy that have a new COMPONENT.
        /// </summary>
        [BurstCompile]
        private struct ManageComponentAdditionJob : IJobChunk
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public ArchetypeChunkComponentType<COMPONENT> ComponentChunk;
            [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
            [ReadOnly] public COMPONENT_REACTOR Reactor;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<COMPONENT> components = chunk.GetNativeArray(ComponentChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; ++i)
                {
                    // Calls the mathod and reassign the COMPONENT to take into account any modification that may have accured during the method call.
                    COMPONENT component = components[i];
                    Reactor.ComponentAdded(ref component);
                    components[i] = component;
                    // Add the system state component and set it's value that on the next frame, the ManageComponentValueChangeJob can handle any change in the COMPONENT value.
                    EntityCommandBuffer.AddComponent<StateComponent>(chunkIndex, entities[i]);
                    EntityCommandBuffer.SetComponent(chunkIndex, entities[i], new StateComponent() { Value = component });
                }
            }
        }
        /// <summary>
        /// This system call the COMPONENT_REACTOR.ComponentRemoved method on all enttiy that were strip down of their COMPONENT.
        /// </summary>
        [BurstCompile]
        private struct ManageComponentRemovalJob : IJobChunk
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            [ReadOnly] public ArchetypeChunkComponentType<StateComponent> StateComponentChunk;
            [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
            [ReadOnly] public COMPONENT_REACTOR Reactor;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<StateComponent> stateComponents = chunk.GetNativeArray(StateComponentChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; ++i)
                {
                    // Calls the mathod with the last know copy of the component, this copy is read only has the component will be remove by hte end of the frame.
                    StateComponent stateComponent = stateComponents[i];
                    Reactor.ComponentRemoved(in stateComponent.Value);
                    // Remove the state component so that the entiyt can be destroyed or listen again for COMPONENT addition.
                    EntityCommandBuffer.RemoveComponent<StateComponent>(chunkIndex, entities[i]);
                }
            }
        }
        /// <summary>
        /// This system call the COMPONENT_REACTOR.ComponentValueChanged method on all entity that had their COMPONENT value changed.
        /// </summary>
        [BurstCompile]
        private struct ManageComponentValueChangeJob : IJobChunk
        {
            public ArchetypeChunkComponentType<COMPONENT> ComponentChunk;
            public ArchetypeChunkComponentType<StateComponent> StateComponentChunk;
            [ReadOnly] public COMPONENT_REACTOR Reactor;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<COMPONENT> components = chunk.GetNativeArray(ComponentChunk);
                NativeArray<StateComponent> stateComponents = chunk.GetNativeArray(StateComponentChunk);
                for (int i = 0; i < chunk.Count; ++i)
                {
                    // Chaeck if the value changed since last frame.
                    StateComponent stateComponent = stateComponents[i];
                    COMPONENT component = components[i];
                    // If it did not change, move to the next entity in chunk.
                    if (ByteBufferUtility.AreEqualStruct(stateComponent.Value, component)) continue;
                    // If it did change, call the method with the new value and the old value (from the last know copy of the COMPONENT)
                    Reactor.ComponentValueChanged(ref component, in stateComponent.Value);
                    // Ressign the COMPONENT to take into account any modification that may have accured during the method call.
                    components[i] = component;
                    // Update the copy of the COMPONENT.
                    stateComponent.Value = component;
                    stateComponents[i] = stateComponent;
                }
            }
        }

    }

}