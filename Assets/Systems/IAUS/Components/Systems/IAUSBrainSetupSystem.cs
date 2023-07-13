using Unity.Entities;
using IAUS.ECS.Component;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst.Intrinsics;

//TODO Complete Rewrite

namespace IAUS.ECS.Systems
{
    public partial class IAUSBrainSetupSystem : SystemBase
    {
        EntityQuery Starter;
        EntityQuery patrolStateEntity;
        EntityQuery wanderStateEntity;

        EntityQuery traverseStateEntity;

        EntityQuery waitStateEntity;
        //        EntityQuery _GetInRangeStateEntity;
        //        EntityQuery _MoveToTargetStateEntity;
                EntityQuery attackStateEntity;
        //        EntityQuery _FleeStateEntity;
        //        EntityQuery _gatherStateEntity;
        //        EntityQuery _terrorStateEntity;

        //        EntityQuery _CallForHelp { get; set; }

        //        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            patrolStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(TravelWaypointBuffer)), ComponentType.ReadOnly(typeof(SetupBrainTag)),
                        ComponentType.ReadWrite(typeof(StateBuffer)),ComponentType.ReadWrite(typeof(Patrol)) ,ComponentType.ReadOnly(typeof(LocalTransform)) },
            });
            wanderStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)),ComponentType.ReadWrite(typeof(StateBuffer)),
                    ComponentType.ReadWrite(typeof(WanderQuadrant)) ,ComponentType.ReadOnly(typeof(LocalTransform)) },
            });
            traverseStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(TravelWaypointBuffer)), ComponentType.ReadOnly(typeof(SetupBrainTag)),
                        ComponentType.ReadWrite(typeof(StateBuffer)),ComponentType.ReadWrite(typeof(Traverse)) ,ComponentType.ReadOnly(typeof(LocalTransform)) },
            });
            waitStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)),
                                    ComponentType.ReadWrite(typeof(StateBuffer)), ComponentType.ReadWrite(typeof(Wait)) }
            });
            //            _GetInRangeStateEntity = GetEntityQuery(new EntityQueryDesc()
            //            { 
            //                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)), ComponentType.ReadWrite(typeof(StateBuffer)),
            //                    ComponentType.ReadWrite(typeof(StayInRange))}
            //            }
            //                );
            //            _MoveToTargetStateEntity = GetEntityQuery(new EntityQueryDesc()
            //            {
            //                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)), ComponentType.ReadWrite(typeof(StateBuffer)),
            //                    ComponentType.ReadWrite(typeof(StayInRange))}
            //            } );

            attackStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)), ComponentType.ReadWrite(typeof(StateBuffer)),
                                ComponentType.ReadWrite(typeof(AttackState)) }
            });
            //            _FleeStateEntity = GetEntityQuery(new EntityQueryDesc()
            //            {
            //                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)), ComponentType.ReadWrite(typeof(StateBuffer)),
            //                    ComponentType.ReadWrite(typeof(RetreatCitizen))}
            //            });
            //            _gatherStateEntity = GetEntityQuery(new EntityQueryDesc()
            //            {
            //                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)), ComponentType.ReadWrite(typeof(StateBuffer)),
            //                    ComponentType.ReadWrite(typeof(GatherResourcesState))}
            //            });

            //            _CallForHelp = GetEntityQuery(new EntityQueryDesc()
            //            {
            //                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)), ComponentType.ReadWrite(typeof(StateBuffer)),
            //                    ComponentType.ReadWrite(typeof(SpawnDefendersState))}
            //            });
            //            _terrorStateEntity = GetEntityQuery(new EntityQueryDesc()
            //            {
            //                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)), ComponentType.ReadWrite(typeof(StateBuffer)),
            //                    ComponentType.ReadWrite(typeof(TerrorizeAreaState))}
            //            });

            Starter = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { typeof(IAUSBrain), typeof(StateBuffer), typeof(SetupBrainTag) }

            });

        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();

            systemDeps = new AddMovementState<Patrol>()
            {
                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
                MovementChunk = GetComponentTypeHandle<Patrol>(false),
                PatrolBufferChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true),
                ToWorldChunk = GetComponentTypeHandle<LocalTransform>(true)
            }.ScheduleParallel(patrolStateEntity, systemDeps);

            ecbSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new AddMovementState<Traverse>()
            {
                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
                MovementChunk = GetComponentTypeHandle<Traverse>(false),
                PatrolBufferChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true),
                ToWorldChunk = GetComponentTypeHandle<LocalTransform>(true)
            }.ScheduleParallel(traverseStateEntity, systemDeps);
            ecbSystem.AddJobHandleForProducer(systemDeps);
            
            systemDeps = new AddWanderState() { 
                ECB = ecbSingleton.CreateCommandBuffer(World.Unmanaged).AsParallelWriter()
            }.ScheduleParallel(wanderStateEntity, systemDeps);
            ecbSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AddWaitState()
            {
                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
                WaitChunk = GetComponentTypeHandle<Wait>(false),
            }
            .ScheduleParallel(waitStateEntity, systemDeps);
            ecbSystem.AddJobHandleForProducer(systemDeps);


            //            systemDeps = new AddStayInRange()
            //            {
            //                entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            //                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
            //                StayInRangeChunk = GetComponentTypeHandle<StayInRange>(false),
            //                EntityChunk = GetEntityTypeHandle(),
            //                HealthRatio = GetComponentDataFromEntity<CharacterHealthConsideration>()
            //            }.ScheduleParallel(_GetInRangeStateEntity, systemDeps);

            //            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            //            systemDeps = new AddMoveToTargetState() 
            //            {
            //                entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            //                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
            //                MoveToTargetChunk = GetComponentTypeHandle<MoveToTarget>(false),
            //                EntityChunk = GetEntityTypeHandle(),
            //                HealthRatio = GetComponentDataFromEntity<CharacterHealthConsideration>()
            //            }.ScheduleParallel(_MoveToTargetStateEntity ,systemDeps);
            //            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AddAttacks()
            {
                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
                AttackChunk = GetComponentTypeHandle<AttackState>(false),
            }.ScheduleParallel(attackStateEntity, systemDeps);
            ecbSystem.AddJobHandleForProducer(systemDeps);

            //            systemDeps = new AddRetreatState()
            //            {
            //                entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            //                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
            //                FleeChunk = GetComponentTypeHandle<RetreatCitizen>(false),
            //                EntityChunk = GetEntityTypeHandle(),
            //                HealthRatio = GetComponentDataFromEntity<CharacterHealthConsideration>()
            //            }.ScheduleParallel(_FleeStateEntity, systemDeps);
            //            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            //            systemDeps = new AddGatherResourcesState()
            //            {
            //                entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            //                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
            //                GatherChunk = GetComponentTypeHandle<GatherResourcesState>(false)
            //            }.ScheduleParallel(_gatherStateEntity, systemDeps);
            //            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            //            systemDeps = new AddSpawnDefendersState()
            //            {
            //                entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            //                StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
            //                EntityChunk = GetEntityTypeHandle(),
            //                SpawnChunk = GetComponentTypeHandle<SpawnDefendersState>(false)
            //            }.ScheduleParallel(_CallForHelp, systemDeps);
            //            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            //            //TODO figure this out
            //            //systemDeps = new AddTerrorArea()
            //            //{
            //            //    entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            //            //    StateBufferChunk = GetBufferTypeHandle<StateBuffer>(false),
            //            //    EntityChunk = GetEntityTypeHandle(),
            //            //    TerrorChunk = GetComponentTypeHandle<TerrorizeAreaState>(false),
            //            //    Distance = GetComponentDataFromEntity<DistanceToConsideration>(true),
            //            //}.ScheduleParallel(_terrorStateEntity, systemDeps);
            //            //entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            // This is to be the last job of this system
            systemDeps = new RemoveSetupTag()
            {
                entityCommandBuffer = ecbSystem.CreateCommandBuffer().AsParallelWriter(),
                EntityChunk = GetEntityTypeHandle()
            }.ScheduleParallel(Starter, systemDeps);
            ecbSystem.AddJobHandleForProducer(systemDeps);

   

            Dependency = systemDeps;
        }


        [BurstCompile]
        public struct RemoveSetupTag : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> Entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    entityCommandBuffer.RemoveComponent<SetupBrainTag>(unfilteredChunkIndex, Entities[i]);
                }
            }


        }
    }
}