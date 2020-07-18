
using IAUS.Core;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using IAUS.ECS2.BackGround.Raycasting;
using InfluenceMap;
using IAUS.ECS2.IAUSSetup;

namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_Initialization))]
    public partial class SetupIAUS : SystemBase
    {
        private EntityQuery _partolStateEntity;
        private EntityQuery _WaitStateEntity;
        private EntityQuery _followCharEntity;
            EntityCommandBufferSystem _entityCommandBufferSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _partolStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(PatrolBuffer)), ComponentType.ReadOnly(typeof(CreateAIBufferTag)),
                        ComponentType.ReadWrite(typeof(StateBuffer)), ComponentType.ReadWrite(typeof(Patrol)) }

            });
            _WaitStateEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(CreateAIBufferTag)),
                        ComponentType.ReadWrite(typeof(StateBuffer)), ComponentType.ReadWrite(typeof(WaitTime)) }
            });
            _followCharEntity = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(CreateAIBufferTag)), 
                        ComponentType.ReadWrite(typeof(StateBuffer)), ComponentType.ReadWrite(typeof(FollowCharacter)) }
            });
        }

        protected override void OnUpdate()
        {

            EntityCommandBuffer.Concurrent entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
       
            JobHandle systemDeps = Dependency;
            
            systemDeps = new SetupPatrolState()
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                EntityChunk = GetArchetypeChunkEntityType(),
                health = GetComponentDataFromEntity<HealthConsideration>(true),
                Distance = GetComponentDataFromEntity<DistanceToConsideration>(true),
                TimeC = GetComponentDataFromEntity<TimerConsideration>(true),
                Influences = GetComponentDataFromEntity<InfluenceValues>(true),
                PatrolBufferChunk = GetArchetypeChunkBufferType<PatrolBuffer>(false),
                PatrolChunk = GetArchetypeChunkComponentType<Patrol>(false),
                StateBufferChunk = GetArchetypeChunkBufferType<StateBuffer>(false)
            }.ScheduleParallel(_partolStateEntity ,systemDeps);
        //    _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new SetupWaitState() {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                EntityChunk = GetArchetypeChunkEntityType(),
                health = GetComponentDataFromEntity<HealthConsideration>(true),
                Distance = GetComponentDataFromEntity<DistanceToConsideration>(true),
                TimeC = GetComponentDataFromEntity<TimerConsideration>(true),
                StateBufferChunk = GetArchetypeChunkBufferType<StateBuffer>(false)

            } .Schedule(_WaitStateEntity, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new SetupFollowState()
            {
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                EntityChunk = GetArchetypeChunkEntityType(),
                health = GetComponentDataFromEntity<HealthConsideration>(true),
                Influences = GetComponentDataFromEntity<InfluenceValues>(true),
               FollowChunk= GetArchetypeChunkComponentType<FollowCharacter>(false),
                StateBufferChunk = GetArchetypeChunkBufferType<StateBuffer>(false)
            }.ScheduleParallel(_followCharEntity, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);



            // This is to be the last job of this system
             systemDeps = Entities
                .WithNativeDisableParallelForRestriction(entityCommandBuffer)

                .ForEach((Entity entity, int nativeThreadIndex, in CreateAIBufferTag Tag) =>
                {
                    entityCommandBuffer.RemoveComponent<CreateAIBufferTag>(nativeThreadIndex, entity);

                })
                .Schedule(systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
         
            Dependency = systemDeps;

        }

    }


}
