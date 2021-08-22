using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using IAUS.ECS2.Component;
using Unity.Entities;
using Unity.Burst;
using UnityEngine.AI;
using Components.MovementSystem;
using DreamersInc.InflunceMapSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, IAUS.ECS2.Systems.Reactive.RetreatCitizenTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, IAUS.ECS2.Systems.Reactive.RetreatCitizenTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, IAUS.ECS2.Systems.Reactive.RetreatCitizenTagReactor>.ManageComponentRemovalJob))]


namespace IAUS.ECS2.Systems.Reactive
{
    public struct RetreatCitizenTagReactor : IComponentReactorTagsForAIStates<RetreatActionTag, RetreatCitizen>
    {
        public void ComponentAdded(Entity entity, ref RetreatActionTag newComponent, ref RetreatCitizen AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref RetreatCitizen AIStateCompoment, in RetreatActionTag oldComponent)
        {

                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
        }

        public void ComponentValueChanged(Entity entity, ref RetreatActionTag newComponent, ref RetreatCitizen AIStateCompoment, in RetreatActionTag oldComponent)
        {
        }

        public class RetreatReactiveSystem : AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, RetreatCitizenTagReactor>
        {
            protected override RetreatCitizenTagReactor CreateComponentReactor()
            {
                return new RetreatCitizenTagReactor();
            }
        }
    }

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]

    public class RetreatMovement : SystemBase
    {
        private EntityQuery _componentAddedQuery;
        private EntityQuery _componentAddedQueryWithWait;

        private EntityQuery _componentRemovedQuery;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        float interval = 0
            ;
        bool runUpdate => interval <= 0.0f;
        protected override void OnCreate()
        {
            base.OnCreate();
            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)), ComponentType.ReadWrite(typeof(RetreatActionTag)), ComponentType.ReadWrite(typeof(Movement)),
                 ComponentType.ReadOnly(typeof(LocalToWorld))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, RetreatCitizenTagReactor>.StateComponent)) }
            });
            _componentAddedQueryWithWait= GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)), ComponentType.ReadWrite(typeof(RetreatActionTag)), ComponentType.ReadWrite(typeof(Wait)),
                 ComponentType.ReadOnly(typeof(LocalToWorld))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, RetreatCitizenTagReactor>.StateComponent)) }
            });
            _componentRemovedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)),  ComponentType.ReadWrite(typeof(Movement))
                , ComponentType.ReadOnly(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, RetreatCitizenTagReactor>.StateComponent)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(RetreatActionTag)) }

            });


            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            if (runUpdate)
            {
                systemDeps = new RetreatMovementUpdate()
                {
                    RetreatChunk = GetComponentTypeHandle<RetreatCitizen>(true),
                    MovementChunk = GetComponentTypeHandle<Movement>(false)
                }.ScheduleParallel(_componentAddedQueryWithWait, systemDeps);


                _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
                interval = 4.0f;
            }
            else
            { 
                interval -= 1 / 60.0f;
            }
            systemDeps = new OnCompletionUpdateJob()
            {
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                WaitChunk = GetComponentTypeHandle<Wait>(false),
                Buffer= GetBufferFromEntity<PatrolWaypointBuffer>(false),
                CanPatrol = GetComponentDataFromEntity<Patrol>(false),
                EntityChunk = GetEntityTypeHandle(),
                RetreatChunk = GetComponentTypeHandle<RetreatCitizen>(false),
                ToWorldChunk = GetComponentTypeHandle<LocalToWorld>(true)

            }.ScheduleParallel(_componentRemovedQuery, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;

        }

        //TODO make generic
        [BurstCompile]
        public struct RetreatMovementUpdate : IJobChunk
        {
            public ComponentTypeHandle<Movement> MovementChunk;
            [ReadOnly]public ComponentTypeHandle<RetreatCitizen> RetreatChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> toWorldChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Movement> movements = chunk.GetNativeArray(MovementChunk);
                NativeArray<RetreatCitizen> retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<LocalToWorld> transforms = chunk.GetNativeArray(toWorldChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Movement move = movements[i];
                    start:
                    if (NavMesh.SamplePosition(retreats[i].LocationOfLowestThreat, out NavMeshHit hit, 6.0f, NavMesh.AllAreas))
                    {

                        move.TargetLocation = hit.position;
                        move.CanMove = true;
                        move.SetTargetLocation = true;
                    }
                    else
                    {
                        goto start;
                    }
                    movements[i] = move;

                }

            }
        }
        [BurstCompile]
        public struct OnCompletionUpdateJob : IJobChunk
        {
            public ComponentTypeHandle<Wait> WaitChunk;
            public ComponentTypeHandle<Movement> MovementChunk;
            public ComponentTypeHandle<RetreatCitizen> RetreatChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> ToWorldChunk;

           [NativeDisableParallelForRestriction] public ComponentDataFromEntity<Patrol> CanPatrol;
            [ReadOnly]public BufferFromEntity<PatrolWaypointBuffer> Buffer;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Wait> Waits = chunk.GetNativeArray(WaitChunk);
                NativeArray<Movement> Moves = chunk.GetNativeArray(MovementChunk);
                NativeArray<RetreatCitizen> retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                NativeArray<LocalToWorld> ToWorlds = chunk.GetNativeArray(ToWorldChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Wait wait = Waits[i];
                    Movement move = Moves[i];
                    //TODO create a hide state 
                     //   wait.Timer = retreats[i].HideTime;
                    if (CanPatrol.HasComponent(entities[i])) {
                        Patrol patrol = CanPatrol[entities[i]];
                        //TODO  make threat and Proximity Thresholds a variable of the entity
                        if (patrol.CurWaypoint.InfluenceAtPosition.y > .7 && patrol.CurWaypoint.InfluenceAtPosition.y < .75f) 
                        {
                            //move to the next point
                            patrol.WaypointIndex++;
                            if (patrol.WaypointIndex >= patrol.NumberOfWayPoints)
                                patrol.WaypointIndex = 0;

                            patrol.CurWaypoint = Buffer[entities[i]][patrol.WaypointIndex].WayPoint;

                            patrol.StartingDistance = Vector3.Distance(ToWorlds[i].Position, patrol.CurWaypoint.Position);

                            CanPatrol[entities[i]] = patrol;
                        }
                    }
                    move.CanMove = false;

                    Waits[i] = wait;
                    Moves[i] = move;
                }
            }
        }

    }
    
}