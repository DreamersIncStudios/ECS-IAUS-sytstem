using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace Utilities.ReactiveSystem

{
    public interface AIComponentReactor<COMPONENT>
    {
        void ComponentAdded(Entity entity, ref COMPONENT newComponent,  ref EntityCommandBuffer.Concurrent ECB, ref int ChunkID);
        void ComponentValueChanged(ref Entity entity, ref COMPONENT newComponent, in COMPONENT oldComponent, ref EntityCommandBuffer.Concurrent ECB, ref int ChunkID);

    }
    public abstract class AIReactorSystem<COMPONENT, AI_REACTOR> : SystemBase
         where COMPONENT : unmanaged, IComponentData
    where AI_REACTOR : struct, AIComponentReactor<COMPONENT>
    {
        /// <summary>
        /// Struct implmenting IComponentReactor<COMPONENT> that implements the behavior when COMPONENT is added, removed or changed value.
        /// </summary>
        private AI_REACTOR _reactor;

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

        private bool _isZeroSized;

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
            _reactor = CreateComponentRactor();

            int m_TypeIndex = TypeManager.GetTypeIndex<COMPONENT>();
            _isZeroSized = TypeManager.GetTypeInfo(m_TypeIndex).IsZeroSized;

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

        /// <summary>
        /// Create the reactor struct that implements the behavior when COMPONENT is added, removed or changed value.
        /// </summary>
        /// <returns>COMPONENT_REACTOR</returns>
        protected abstract AI_REACTOR CreateComponentRactor();

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
        private struct ManageComponentAdditionJob : IJobChunk
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public ArchetypeChunkComponentType<COMPONENT> ComponentChunk;
            public bool IsZeroSized;
            [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
            [ReadOnly] public AI_REACTOR Reactor;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<COMPONENT> components = IsZeroSized ? default : chunk.GetNativeArray(ComponentChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);

                for (int i = 0; i < chunk.Count; ++i)
                {
                    // Calls the mathod and reassign the COMPONENT to take into account any modification that may have accured during the method call.
                    COMPONENT component = IsZeroSized ? default : components[i];
                    Entity entity = entities[i];

                    Reactor.ComponentAdded(entity, ref component, ref EntityCommandBuffer, ref chunkIndex);
                    if (!IsZeroSized)
                    {
                        components[i] = component;

                    }
                    // Add the system state component and set it's value that on the next frame, the ManageComponentValueChangeJob can handle any change in the COMPONENT value.
                    EntityCommandBuffer.AddComponent<StateComponent>(chunkIndex, entities[i]);
                    EntityCommandBuffer.SetComponent(chunkIndex, entities[i], new StateComponent() { Value = component });
                }

            }
        }


        /// <summary>
        /// This system call the COMPONENT_REACTOR.ComponentValueChanged method on all entity that had their COMPONENT value changed.
        /// </summary>
        [BurstCompile]
        private struct ManageComponentValueChangeJob : IJobChunk
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;

            public ArchetypeChunkComponentType<COMPONENT> ComponentChunk;
            public ArchetypeChunkComponentType<StateComponent> StateComponentChunk;
            [ReadOnly] public AI_REACTOR Reactor;
            [ReadOnly] public ArchetypeChunkEntityType EntityChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {

                NativeArray<COMPONENT> components = chunk.GetNativeArray(ComponentChunk);
                NativeArray<StateComponent> stateComponents = chunk.GetNativeArray(StateComponentChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);

                for (int i = 0; i < chunk.Count; ++i)
                {
                    // Chaeck if the value changed since last frame.
                    StateComponent stateComponent = stateComponents[i];
                    COMPONENT component = components[i];
                    Entity entity = entities[i];

                    // If it did not change, move to the next entity in chunk.
                    if (ByteBufferUtility.AreEqualStruct(stateComponent.Value, component)) continue;
                    // If it did change, call the method with the new value and the old value (from the last know copy of the COMPONENT)
                    Reactor.ComponentValueChanged(ref entity, ref component, in stateComponent.Value, ref EntityCommandBuffer, ref chunkIndex);

                    // Ressign the COMPONENT to take into account any modification that may have accured during the method call.
                    components[i] = component;

                    // Update the copy of the COMPONENT.
                    stateComponent.Value = component;
                    stateComponents[i] = stateComponent;
                    
                }

            }
        }



        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            // There is no point in looking for change in a component that has no data.
            if (!_isZeroSized)
            {
                systemDeps = new ManageComponentValueChangeJob()
                {
                    ComponentChunk = GetArchetypeChunkComponentType<COMPONENT>(false),
                    StateComponentChunk = GetArchetypeChunkComponentType<StateComponent>(false),
                    Reactor = _reactor,
                    EntityChunk = GetArchetypeChunkEntityType(),
                    EntityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()

                }.ScheduleParallel(_componentValueChangedQuery, systemDeps);
            }
            systemDeps = new ManageComponentAdditionJob()
            {
                ComponentChunk = GetArchetypeChunkComponentType<COMPONENT>(false),
                EntityChunk = GetArchetypeChunkEntityType(),
                EntityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                Reactor = _reactor,
                IsZeroSized = _isZeroSized
            }.ScheduleParallel(_componentAddedQuery, systemDeps);



            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
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

}

