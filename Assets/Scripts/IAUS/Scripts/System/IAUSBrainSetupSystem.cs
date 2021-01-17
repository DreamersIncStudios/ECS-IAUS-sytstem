using Unity.Entities;
using IAUS.ECS2.Component;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine;
namespace IAUS.ECS2.Systems
{
    public class IAUSBrainSetupSystem : SystemBase
    {
        EntityQuery Starter;
        EntityQuery _partolStateEntity;
        EntityQuery _waitStateEntity;
        EntityQuery _GetInRangeStateEntity;
        EntityQuery _MoveToTargetStateEntity;
        EntityQuery _AttackMeleeStateEntity;
        EntityQuery _FleeStateEntity;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            _partolStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(PatrolWaypointBuffer)), ComponentType.ReadOnly(typeof(SetupBrainTag)),
                        ComponentType.ReadWrite(typeof(StateBuffer)), ComponentType.ReadWrite(typeof(Patrol)) }

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

            _AttackMeleeStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)), ComponentType.ReadWrite(typeof(StateBuffer)),
                    ComponentType.ReadWrite(typeof(MeleeAttackTarget))}
            });
            _FleeStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag)), ComponentType.ReadWrite(typeof(StateBuffer)),
                    ComponentType.ReadWrite(typeof(Retreat))}
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
            systemDeps = new AddPatrolState() {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                StateBufferChunk = GetArchetypeChunkBufferType<StateBuffer>(false),
                EntityChunk = GetArchetypeChunkEntityType(),
                PatrolChunk = GetArchetypeChunkComponentType<Patrol>(false),
                Distance = GetComponentDataFromEntity<DistanceToConsideration>(true),
                PatrolBufferChunk = GetArchetypeChunkBufferType<PatrolWaypointBuffer>(true)
            }
            .ScheduleParallel(_partolStateEntity, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AddWaitState() {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                StateBufferChunk = GetArchetypeChunkBufferType<StateBuffer>(false),
                EntityChunk = GetArchetypeChunkEntityType(),
                Distance = GetComponentDataFromEntity<DistanceToConsideration>(true),
                WaitChunk = GetArchetypeChunkComponentType<Wait>(false),
            }
            .ScheduleParallel( _waitStateEntity, systemDeps);
            
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AddStayInRange()
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                StateBufferChunk = GetArchetypeChunkBufferType<StateBuffer>(false),
                StayInRangeChunk = GetArchetypeChunkComponentType<StayInRange>(false),
                EntityChunk = GetArchetypeChunkEntityType(),
                HealthRatio = GetComponentDataFromEntity<CharacterHealthConsideration>()
            }.ScheduleParallel(_GetInRangeStateEntity, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AddMoveToTargetState() 
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                StateBufferChunk = GetArchetypeChunkBufferType<StateBuffer>(false),
                MoveToTargetChunk = GetArchetypeChunkComponentType<MoveToTarget>(false),
                EntityChunk = GetArchetypeChunkEntityType(),
                HealthRatio = GetComponentDataFromEntity<CharacterHealthConsideration>()
            }.ScheduleParallel(_MoveToTargetStateEntity ,systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AddAttackTargetState()
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                StateBufferChunk = GetArchetypeChunkBufferType<StateBuffer>(false),
                AttackTargetChunk = GetArchetypeChunkComponentType<MeleeAttackTarget>(false),
                EntityChunk = GetArchetypeChunkEntityType(),
                HealthRatio = GetComponentDataFromEntity<CharacterHealthConsideration>()
            }.ScheduleParallel(_AttackMeleeStateEntity, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            
            systemDeps = new AddFleeState()
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                StateBufferChunk = GetArchetypeChunkBufferType<StateBuffer>(false),
                FleeChunk = GetArchetypeChunkComponentType<Retreat>(false),
                EntityChunk = GetArchetypeChunkEntityType(),
                HealthRatio = GetComponentDataFromEntity<CharacterHealthConsideration>()
            }.ScheduleParallel(_FleeStateEntity, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            // This is to be the last job of this system

            systemDeps = new RemoveSetupTag()
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                EntityChunk = GetArchetypeChunkEntityType()
            }.ScheduleParallel(Starter, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }


        [BurstCompile]
        public struct RemoveSetupTag : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
            public EntityCommandBuffer.Concurrent entityCommandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                 NativeArray<Entity> Entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    entityCommandBuffer.RemoveComponent<SetupBrainTag>(chunkIndex, Entities[i]);
                }
            }
        }
    }
}