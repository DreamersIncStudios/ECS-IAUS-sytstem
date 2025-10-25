using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Utilities.ReactiveSystem
{
    public interface IComponentReactorTagsForAIBuffer<COMPONENT, AICOMPONENT>
        where COMPONENT : unmanaged, IComponentData
        where AICOMPONENT : unmanaged, IBufferElementData
    {
        void ComponentAdded(Entity entity, ref COMPONENT newComponent, DynamicBuffer<AICOMPONENT> aiBuffer);
        void ComponentRemoved(Entity entity, DynamicBuffer<AICOMPONENT> aiBuffer, in COMPONENT oldComponent);
    }

    public abstract partial class AIReactiveSystemBuffer<COMPONENT, AICOMPONENT, COMPONENT_REACTOR> : SystemBase
        where COMPONENT : unmanaged, IComponentData
        where AICOMPONENT : unmanaged, IBufferElementData
        where COMPONENT_REACTOR : struct, IComponentReactorTagsForAIBuffer<COMPONENT, AICOMPONENT>
    {
        /// <summary>
        /// Struct implementing IComponentReactor<COMPONENT> that implements the behavior when COMPONENT is added, removed or changed value.
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

        // private EntityQuery _componentValueChangedQuery;
        /// <summary>
        /// EntityCommandBufferSystem used to add and remove the StateComponent.
        /// </summary>
        private EntityCommandBufferSystem _entityCommandBufferSystem;
        /// <summary>
        /// The state component for this reactive system.
        /// It contains a copy of the COMPONENT data.
        /// </summary>
        public struct StateComponent : ICleanupComponentData
        {
            public COMPONENT Value;
        }
        protected override void OnCreate()
        {
            base.OnCreate();

            RequireForUpdate<RunningTag>();

            _reactor = CreateComponentReactor();

            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadWrite(typeof(COMPONENT)),
                    ComponentType.ReadWrite(typeof(AICOMPONENT))
                },
                None = new[]
                {
                    ComponentType.ReadOnly(typeof(StateComponent))
                }
            });
            _componentRemovedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly(typeof(StateComponent)),
                    ComponentType.ReadWrite(typeof(AICOMPONENT))
                },
                None = new[]
                {
                    ComponentType.ReadOnly(typeof(COMPONENT))
                }
            });

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
            return World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        /// <summary>
        /// This system calls the COMPONENT_REACTOR.ComponentAdded method on all entity that have a new COMPONENT.
        /// </summary>
        [BurstCompile]
        public struct ManageComponentAdditionJob : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            public ComponentTypeHandle<COMPONENT> ComponentChunk;

            public BufferTypeHandle<AICOMPONENT> AIComponentChunk;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            [ReadOnly] public COMPONENT_REACTOR Reactor;
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<COMPONENT> components = chunk.GetNativeArray(ref ComponentChunk);
                BufferAccessor<AICOMPONENT> aiComponents = chunk.GetBufferAccessor(ref AIComponentChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; ++i)
                {
                    // Calls the method and reassign the COMPONENT to take into account any modification that may have accrued during the method call.
                    COMPONENT component = components[i];
                    DynamicBuffer<AICOMPONENT> AIcomponent = aiComponents[i];
                    Entity entity = entities[i];
                    Reactor.ComponentAdded(entity, ref component, AIcomponent);
                    components[i] = component;
           
                    // Add the system state component and set its value that on the next frame, the ManageComponentValueChangeJob can handle any change in the COMPONENT value.
                    EntityCommandBuffer.AddComponent<StateComponent>(unfilteredChunkIndex, entities[i]);
                    EntityCommandBuffer.SetComponent(unfilteredChunkIndex, entities[i], new StateComponent()
                    {
                        Value = component
                    });
                }
            }
        }

        /// <summary>
        /// This system calls the COMPONENT_REACTOR.ComponentRemoved method on all entity that were strip down of their COMPONENT.
        /// </summary>
        [BurstCompile]
        public struct ManageComponentRemovalJob : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            public BufferTypeHandle<AICOMPONENT> AIComponentChunk;
            [ReadOnly] public ComponentTypeHandle<StateComponent> StateComponentChunk;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            [ReadOnly] public COMPONENT_REACTOR Reactor;



            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<StateComponent> stateComponents = chunk.GetNativeArray(ref StateComponentChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                BufferAccessor<AICOMPONENT> aiComponents = chunk.GetBufferAccessor(ref AIComponentChunk);
                for (int i = 0; i < chunk.Count; ++i)
                {
                    // Calls the method with the last know copy of the component, this copy is read only has the component will be remove by the end of the frame.
                    Entity entity = entities[i];
                    DynamicBuffer<AICOMPONENT> AIcomponent = aiComponents[i];
                    StateComponent stateComponent = stateComponents[i];
                    Reactor.ComponentRemoved(entity, AIcomponent, in stateComponent.Value);
                
                    // Remove the state component so that the entity can be destroyed or listen again for COMPONENT addition.
                    EntityCommandBuffer.RemoveComponent<StateComponent>(unfilteredChunkIndex, entities[i]);
                }
            }
        }
        
               protected override void OnUpdate()
        {

            JobHandle systemDeps = Dependency;

            systemDeps = new ManageComponentAdditionJob()
            {
                ComponentChunk = GetComponentTypeHandle<COMPONENT>(false),
                AIComponentChunk = GetBufferTypeHandle<AICOMPONENT>(false),
                EntityChunk = GetEntityTypeHandle(),
                EntityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                Reactor = _reactor,

            }.ScheduleParallel(_componentAddedQuery, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new ManageComponentRemovalJob()
            {
                AIComponentChunk = GetBufferTypeHandle<AICOMPONENT>(false),
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
    
    
}

