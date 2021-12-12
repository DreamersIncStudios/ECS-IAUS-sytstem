using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using Components.MovementSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<WaitActionTag, Wait, IAUS.ECS.Systems.Reactive.WaitTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<WaitActionTag, Wait, IAUS.ECS.Systems.Reactive.WaitTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<WaitActionTag, Wait, IAUS.ECS.Systems.Reactive.WaitTagReactor>.ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive
{
    public struct WaitTagReactor : IComponentReactorTagsForAIStates<WaitActionTag, Wait>
    {
        public void ComponentAdded(Entity entity, ref WaitActionTag newComponent, ref Wait AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref Wait AIStateCompoment, in WaitActionTag oldComponent)
        {
            if (AIStateCompoment.Complete)
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
            }
            else
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime * 2;

            }
        }

        public void ComponentValueChanged(Entity entity, ref WaitActionTag newComponent, ref Wait AIStateCompoment, in WaitActionTag oldComponent)
        {
            Debug.Log("Change");
        }

        public class WaitReactiveSystem : AIReactiveSystemBase<WaitActionTag, Wait, WaitTagReactor>
        {
            protected override WaitTagReactor CreateComponentReactor()
            {
                return new WaitTagReactor();
            }
        }
    }

    public sealed class WaitSystem : SystemBase
    {
        private EntityQuery _componentRemovedQuery;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _componentRemovedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] {  ComponentType.ReadWrite(typeof(Wait)), ComponentType.ReadWrite(typeof(LocalToWorld))
                , ComponentType.ReadOnly(typeof(TravelWaypointBuffer)), ComponentType.ReadOnly(typeof(AIReactiveSystemBase<WaitActionTag, Wait, WaitTagReactor>.StateComponent)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(WaitActionTag)) },
                Any = new ComponentType[] { ComponentType.ReadOnly(typeof(Patrol)), ComponentType.ReadOnly(typeof(Traverse))}
            });

        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new MovementStateUpdateJob<Patrol>()
            {
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                PatrolChunk = GetComponentTypeHandle<Patrol>(false),
                WaypointChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true),
                ToWorldChunk = GetComponentTypeHandle<LocalToWorld>(true)
            }.ScheduleParallel(_componentRemovedQuery, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            
            systemDeps = new MovementStateUpdateJob<Traverse>()
            {
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                PatrolChunk = GetComponentTypeHandle<Traverse>(false),
                WaypointChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true),
                ToWorldChunk = GetComponentTypeHandle<LocalToWorld>(true)
            }.ScheduleParallel(_componentRemovedQuery, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            Dependency = systemDeps;

        }

        [BurstCompile]
        public struct MovementStateUpdateJob<T> : IJobChunk
            where T: unmanaged, MovementState
        {
            public ComponentTypeHandle<Movement> MovementChunk;
            public ComponentTypeHandle<T> PatrolChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> ToWorldChunk;
            [ReadOnly] public BufferTypeHandle<TravelWaypointBuffer> WaypointChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<T> moveStates = chunk.GetNativeArray(PatrolChunk);
                NativeArray<LocalToWorld> ToWorlds = chunk.GetNativeArray(ToWorldChunk);
                BufferAccessor<TravelWaypointBuffer> WaypointBuffers = chunk.GetBufferAccessor(WaypointChunk);
                if (moveStates.Length == 0)
                    return;
                for (int i = 0; i < chunk.Count; i++)
                {
                    T moveState = moveStates[i];
                    DynamicBuffer<TravelWaypointBuffer> waypointBuffer = WaypointBuffers[i];

                    moveState.WaypointIndex++;
                    if (moveState.WaypointIndex >= moveState.NumberOfWayPoints)
                        moveState.WaypointIndex = 0;

                    moveState.CurWaypoint = waypointBuffer[moveState.WaypointIndex].WayPoint;

                    moveState.StartingDistance = Vector3.Distance(ToWorlds[i].Position, moveState.CurWaypoint.Position);
                    moveStates[i] = moveState;
                }    

            }

        }
    }

}