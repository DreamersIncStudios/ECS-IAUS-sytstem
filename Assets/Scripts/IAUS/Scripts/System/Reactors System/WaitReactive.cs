using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS2.Component;
using Unity.Entities;
using Unity.Burst;
using Components.MovementSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<WaitActionTag, Wait, IAUS.ECS2.Systems.Reactive.WaitTagReactor>.StateComponent))]

namespace IAUS.ECS2.Systems.Reactive
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

    public class WaitSystem : SystemBase
    {
        private EntityQuery _componentRemovedQuery;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _componentRemovedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Patrol)), ComponentType.ReadWrite(typeof(Wait)), ComponentType.ReadWrite(typeof(LocalToWorld))
                , ComponentType.ReadOnly(typeof(PatrolWaypointBuffer)), ComponentType.ReadOnly(typeof(AIReactiveSystemBase<WaitActionTag, Wait, WaitTagReactor>.StateComponent)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(WaitActionTag)) }
            });

        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new PatrolMovementUpdateJob()
            {
                MovementChunk = GetArchetypeChunkComponentType<Movement>(false),
                PatrolChunk = GetArchetypeChunkComponentType<Patrol>(false),
                WaypointChunk = GetArchetypeChunkBufferType<PatrolWaypointBuffer>(true),
                ToWorldChunk = GetArchetypeChunkComponentType<LocalToWorld>(true)
            }.ScheduleParallel(_componentRemovedQuery, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            Dependency = systemDeps;

        }

        [BurstCompile]
        public struct PatrolMovementUpdateJob : IJobChunk
        {
            public ArchetypeChunkComponentType<Movement> MovementChunk;
            public ArchetypeChunkComponentType<Patrol> PatrolChunk;
            [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> ToWorldChunk;
            [ReadOnly] public ArchetypeChunkBufferType<PatrolWaypointBuffer> WaypointChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
                NativeArray<LocalToWorld> ToWorlds = chunk.GetNativeArray(ToWorldChunk);
                BufferAccessor<PatrolWaypointBuffer> WaypointBuffers = chunk.GetBufferAccessor(WaypointChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Patrol patrol = patrols[i];
                    DynamicBuffer<PatrolWaypointBuffer> waypointBuffer = WaypointBuffers[i];
                    {
                        patrol.WaypointIndex++;
                        if (patrol.WaypointIndex >= patrol.NumberOfWayPoints)
                            patrol.WaypointIndex = 0;
                        patrol.CurWaypoint = waypointBuffer[patrol.WaypointIndex].WayPoint;

                        patrol.StartingDistance = Vector3.Distance(ToWorlds[i].Position, patrol.CurWaypoint.Position);
                        patrols[i] = patrol;
                    }


                }
            }

        }
    }

}