using Unity.Entities;
using IAUS.ECS.Component;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.ECS.Systems
{
    public partial class IAUSBrainSetupSystem : SystemBase
    {
        EntityQuery Starter;
        EntityQuery _PatrolStateEntity;
        EntityQuery _traverseStateEntity;

        EntityQuery _waitStateEntity;
        EntityQuery _GetInRangeStateEntity;
        EntityQuery _MoveToTargetStateEntity;
        EntityQuery _AttackStateEntity;
        EntityQuery _FleeStateEntity;
        EntityQuery _gatherStateEntity;
        EntityQuery _CallForHelp { get; set; }

        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            _PatrolStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(TravelWaypointBuffer)), ComponentType.ReadOnly(typeof(SetupBrainTag)),
                        ComponentType.ReadWrite(typeof(StateBuffer)),ComponentType.ReadWrite(typeof(Patrol)) ,ComponentType.ReadOnly(typeof(LocalToWorld)) },
            });

            _traverseStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(TravelWaypointBuffer)), ComponentType.ReadOnly(typeof(SetupBrainTag)),
                        ComponentType.ReadWrite(typeof(StateBuffer)),ComponentType.ReadWrite(typeof(Traverse)) ,ComponentType.ReadOnly(typeof(LocalToWorld)) },
            });
            _waitStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)),
                        ComponentType.ReadWrite(typeof(StateBuffer)), ComponentType.ReadWrite(typeof(Wait)) }
            });
            _GetInRangeStateEntity = GetEntityQuery(new EntityQueryDesc()
            { 
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)), ComponentType.ReadWrite(typeof(StateBuffer)),
                    ComponentType.ReadWrite(typeof(StayInRange))}
            }
                );
            _MoveToTargetStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)), ComponentType.ReadWrite(typeof(StateBuffer)),
                    ComponentType.ReadWrite(typeof(StayInRange))}
            } );

            _AttackStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)), ComponentType.ReadWrite(typeof(StateBuffer)),
                    ComponentType.ReadWrite(typeof(AttackTargetState)) , ComponentType.ReadWrite(typeof(AttackTypeInfo))}
            });
            _FleeStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)), ComponentType.ReadWrite(typeof(StateBuffer)),
                    ComponentType.ReadWrite(typeof(RetreatCitizen))}
            });
            _gatherStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)), ComponentType.ReadWrite(typeof(StateBuffer)),
                    ComponentType.ReadWrite(typeof(GatherResourcesState))}
            });

            _CallForHelp = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)), ComponentType.ReadWrite(typeof(StateBuffer)),
                    ComponentType.ReadWrite(typeof(SpawnDefendersState))}
            });

            Starter = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { typeof(IAUSBrain), typeof(StateBuffer), typeof(SetupBrainTag) }

            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new AddMovementState<Patrol>() {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
                EntityChunk = GetEntityTypeHandle(),
                 MovementChunk = GetComponentTypeHandle<Patrol>(false),
                Distance = GetComponentDataFromEntity<DistanceToConsideration>(true),
                PatrolBufferChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true),
                ToWorldChunk = GetComponentTypeHandle<LocalToWorld>(true)
            }
            .ScheduleParallel(_PatrolStateEntity, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new AddMovementState<Traverse>()
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
                EntityChunk = GetEntityTypeHandle(),
                MovementChunk = GetComponentTypeHandle<Traverse>(false),
                Distance = GetComponentDataFromEntity<DistanceToConsideration>(true),
                PatrolBufferChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true),
                ToWorldChunk = GetComponentTypeHandle<LocalToWorld>(true)
            }
          .ScheduleParallel(_traverseStateEntity, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AddWaitState() {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
                EntityChunk = GetEntityTypeHandle(),
                Distance = GetComponentDataFromEntity<DistanceToConsideration>(true),
                WaitChunk = GetComponentTypeHandle<Wait>(false),
            }
            .ScheduleParallel( _waitStateEntity, systemDeps);
            
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AddStayInRange()
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
                StayInRangeChunk = GetComponentTypeHandle<StayInRange>(false),
                EntityChunk = GetEntityTypeHandle(),
                HealthRatio = GetComponentDataFromEntity<CharacterHealthConsideration>()
            }.ScheduleParallel(_GetInRangeStateEntity, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AddMoveToTargetState() 
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
                MoveToTargetChunk = GetComponentTypeHandle<MoveToTarget>(false),
                EntityChunk = GetEntityTypeHandle(),
                HealthRatio = GetComponentDataFromEntity<CharacterHealthConsideration>()
            }.ScheduleParallel(_MoveToTargetStateEntity ,systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AddAttacks()
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
                 AttackChunk = GetComponentTypeHandle<AttackTargetState>(false),
                EntityChunk = GetEntityTypeHandle(),
                HealthRatio = GetComponentDataFromEntity<CharacterHealthConsideration>()
            }.ScheduleParallel(_AttackStateEntity, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AddRetreatState()
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
                FleeChunk = GetComponentTypeHandle<RetreatCitizen>(false),
                EntityChunk = GetEntityTypeHandle(),
                HealthRatio = GetComponentDataFromEntity<CharacterHealthConsideration>()
            }.ScheduleParallel(_FleeStateEntity, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            
            systemDeps = new AddGatherResourcesState()
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
                EntityChunk = GetEntityTypeHandle(),
                GatherChunk = GetComponentTypeHandle<GatherResourcesState>(false)
            }.ScheduleParallel(_gatherStateEntity, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AddSpawnDefendersState()
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
                EntityChunk = GetEntityTypeHandle(),
                SpawnChunk = GetComponentTypeHandle<SpawnDefendersState>(false)
            }.ScheduleParallel(_CallForHelp, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            // This is to be the last job of this system

            systemDeps = new RemoveSetupTag()
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                EntityChunk = GetEntityTypeHandle()
            }.ScheduleParallel(Starter, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }


        [BurstCompile]
        public struct RemoveSetupTag : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                 NativeArray<Entity> Entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                     Debug.Log("test");

                    entityCommandBuffer.RemoveComponent<SetupBrainTag>(chunkIndex, Entities[i]);
                }
            }
        }
    }
}