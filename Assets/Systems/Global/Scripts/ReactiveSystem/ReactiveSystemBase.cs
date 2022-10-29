using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;


namespace Utilities.ReactiveSystem
{

    public interface IComponentReactorTagsForAIStates<COMPONENT, AICOMPONENT>
    {

        void ComponentAdded(Entity entity, ref COMPONENT newComponent, ref AICOMPONENT AIStateCompoment);
        void ComponentRemoved(Entity entity, ref AICOMPONENT AIStateCompoment, in COMPONENT oldComponent);
        void ComponentValueChanged(Entity entity, ref COMPONENT newComponent, ref AICOMPONENT AIStateCompoment, in COMPONENT oldComponent);
    }


    public abstract partial class AIReactiveSystemBase<COMPONENT, AICOMPONENT, COMPONENT_REACTOR> : SystemBase
        where COMPONENT : unmanaged, IComponentData
        where AICOMPONENT : unmanaged, IComponentData
        where COMPONENT_REACTOR : struct, IComponentReactorTagsForAIStates<COMPONENT, AICOMPONENT>
    {
        /// <summary>
        /// Struct implmenting IComponentReactor<COMPONENT> that implements the behavior when COMPONENT is added, removed or changed value.
        /// </summary>
        private COMPONENT_REACTOR _reactor;
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
       // private EntityQuery _componentValueChangedQuery;
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
            public COMPONENT Value;
        }
        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            _reactor = CreateComponentReactor();

            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(COMPONENT)), ComponentType.ReadWrite(typeof(AICOMPONENT)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(StateComponent)) }
            });
            _componentRemovedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(StateComponent)), ComponentType.ReadWrite(typeof(AICOMPONENT)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(COMPONENT)) }
            });
            //_componentValueChangedQuery = GetEntityQuery(new EntityQueryDesc()
            //{
            //    All = new ComponentType[] { ComponentType.ReadWrite(typeof(COMPONENT)), ComponentType.ReadWrite(typeof(StateComponent)), ComponentType.ReadWrite(typeof(AICOMPONENT)) }
            //});
            _entityCommandBufferSystem = GetCommandBufferSystem();
        }
        /// <summary>
        /// Create the reactor struct that implements the behavior when COMPONENT is added, removed or changed value.
        /// </summary>
        /// <returns>COMPONENT_REACTOR</returns>
        protected abstract COMPONENT_REACTOR CreateComponentReactor();
        /// <summary>
        /// Get the EntityCommandBufferSystem buffer system to use to add and remove the StateComponent.
        /// </summary>
        /// <returns>EntityCommandBufferSystem</returns>
        protected EntityCommandBufferSystem GetCommandBufferSystem()
        {
            return World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        /// <summary>
        /// This system call the COMPONENT_REACTOR.ComponentAdded method on all enttiy that have a new COMPONENT.
        /// </summary>
        [BurstCompile]
        public struct ManageComponentAdditionJob : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            public ComponentTypeHandle<COMPONENT> ComponentChunk;

            public ComponentTypeHandle<AICOMPONENT> AIComponentChunk;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            [ReadOnly] public COMPONENT_REACTOR Reactor;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<COMPONENT> components = chunk.GetNativeArray(ComponentChunk);
                NativeArray<AICOMPONENT> aiComponents = chunk.GetNativeArray(AIComponentChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; ++i)
                {
                    // Calls the mathod and reassign the COMPONENT to take into account any modification that may have accured during the method call.
                    COMPONENT component = components[i];
                    AICOMPONENT AIcomponent = aiComponents[i];
                    Entity entity = entities[i];
                    Reactor.ComponentAdded(entity, ref component, ref AIcomponent);
                    components[i] = component;
                    aiComponents[i] = AIcomponent;
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
        public struct ManageComponentRemovalJob : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            public ComponentTypeHandle<AICOMPONENT> AIComponentChunk;
            [ReadOnly] public ComponentTypeHandle<StateComponent> StateComponentChunk;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            [ReadOnly] public COMPONENT_REACTOR Reactor;

            

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<StateComponent> stateComponents = chunk.GetNativeArray(StateComponentChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                NativeArray<AICOMPONENT> aiComponents = chunk.GetNativeArray(AIComponentChunk);
                for (int i = 0; i < chunk.Count; ++i)
                {
                    // Calls the mathod with the last know copy of the component, this copy is read only has the component will be remove by hte end of the frame.
                    Entity entity = entities[i];
                    AICOMPONENT AIcomponent = aiComponents[i];
                    StateComponent stateComponent = stateComponents[i];
                    Reactor.ComponentRemoved(entity, ref AIcomponent, in stateComponent.Value);

                    aiComponents[i] = AIcomponent;
                    // Remove the state component so that the entiyt can be destroyed or listen again for COMPONENT addition.
                    EntityCommandBuffer.RemoveComponent<StateComponent>(chunkIndex, entities[i]);
                }
            }
        }
        /// <summary>
        /// This system call the COMPONENT_REACTOR.ComponentValueChanged method on all entity that had their COMPONENT value changed.
        /// </summary>
        //[BurstCompile]
        //private struct ManageComponentValueChangeJob : IJobChunk
        //{
        //    public ArchetypeChunkComponentType<COMPONENT> ComponentChunk;
        //    public ArchetypeChunkComponentType<AICOMPONENT> AIComponentChunk;
        //    public ArchetypeChunkComponentType<StateComponent> StateComponentChunk;
        //    [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
        //    [ReadOnly] public COMPONENT_REACTOR Reactor;
        //    public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        //    {
        //        NativeArray<COMPONENT> components = chunk.GetNativeArray(ComponentChunk);
        //        NativeArray<StateComponent> stateComponents = chunk.GetNativeArray(StateComponentChunk);
        //        NativeArray<AICOMPONENT> aiComponents = chunk.GetNativeArray(AIComponentChunk);
        //        NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
        //        for (int i = 0; i < chunk.Count; ++i)
        //        {
        //            // Chaeck if the value changed since last frame.
        //            StateComponent stateComponent = stateComponents[i];
        //            COMPONENT component = components[i];
        //            Entity entity = entities[i];
        //            AICOMPONENT AIcomponent = aiComponents[i];
        //            // If it did not change, move to the next entity in chunk.
        //            if (ByteBufferUtility.AreEqualStruct(stateComponent.Value, component)) continue;
        //            // If it did change, call the method with the new value and the old value (from the last know copy of the COMPONENT)
        //            Reactor.ComponentValueChanged(entity, ref component, ref AIcomponent, in stateComponent.Value);
        //            // Ressign the COMPONENT to take into account any modification that may have accured during the method call.
        //            components[i] = component;
        //            aiComponents[i] = AIcomponent;

        //            // Update the copy of the COMPONENT.
        //            stateComponent.Value = component;
        //            stateComponents[i] = stateComponent;
        //        }
        //    }
        //}    
        protected override void OnUpdate()
        {

            JobHandle systemDeps = Dependency;
            // Removing as it is not used right now .15ms performance hit
            //systemDeps = new ManageComponentValueChangeJob()
            //{
            //    ComponentChunk = GetArchetypeChunkComponentType<COMPONENT>(false),
            //    AIComponentChunk = GetArchetypeChunkComponentType<AICOMPONENT>(false),
            //    StateComponentChunk = GetArchetypeChunkComponentType<StateComponent>(false),
            //    EntityChunk = GetArchetypeChunkEntityType(),
            //    Reactor = _reactor
            //}.ScheduleParallel(_componentValueChangedQuery, systemDeps);

            //_entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new ManageComponentAdditionJob()
            {
                ComponentChunk = GetComponentTypeHandle<COMPONENT>(false),
                AIComponentChunk = GetComponentTypeHandle<AICOMPONENT>(false),
                EntityChunk = GetEntityTypeHandle(),
                EntityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                Reactor = _reactor,

            }.ScheduleParallel(_componentAddedQuery, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new ManageComponentRemovalJob()
            {
                AIComponentChunk = GetComponentTypeHandle<AICOMPONENT>(false),
                StateComponentChunk = GetComponentTypeHandle<StateComponent>(false),
                EntityChunk = GetEntityTypeHandle(),
                EntityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),

                Reactor = _reactor
            }.ScheduleParallel(_componentRemovedQuery, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps.Complete();
            Dependency = systemDeps;
        }
    }
        public static class ByteBufferUtility
        {
            public static bool AreEqualStruct<T>(T frist, T second) where T : unmanaged
            {
                NativeArray<byte> firstArray = ConvertToNativeBytes<T>(frist, Allocator.Temp);
                NativeArray<byte> secondArray = ConvertToNativeBytes<T>(second, Allocator.Temp);
                if (firstArray.Length != secondArray.Length) return false;
                for (int i = 0; i < firstArray.Length; ++i)
                {
                    if (firstArray[i] != secondArray[i]) return false;
                }
                return true;
            }
            private static NativeArray<byte> ConvertToNativeBytes<T>(T value, Allocator allocator) where T : unmanaged
            {
                int size = UnsafeUtility.SizeOf<T>();
                NativeArray<byte> ret = new NativeArray<byte>(size, allocator);
                unsafe
                {
                    UnsafeUtility.CopyStructureToPtr(ref value, ret.GetUnsafePtr());
                }
                return ret;
            }

        }
    

}
