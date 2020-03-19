
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;


namespace IAUS.ECS2
{
    [UpdateBefore(typeof(ConsiderationSystem))]
    public class SetupIAUS : JobComponentSystem
    {

        EntityCommandBufferSystem entityCommandBufferSystem;
        [NativeDisableParallelForRestriction]EntityCommandBuffer commandBuffer;
        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
          
            JobHandle patrolAdd = new PatrolAdd()
            {
                health = GetComponentDataFromEntity<HealthConsideration>(true),
                TimeC = GetComponentDataFromEntity<TimerConsideration>(true),
                Distance = GetComponentDataFromEntity<DistanceToConsideration>(true),
                entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer()
        }.Schedule(this, inputDeps);

            JobHandle waitAdd = new WaitAdd()
            {
                health = GetComponentDataFromEntity<HealthConsideration>(true),
                TimeC = GetComponentDataFromEntity<TimerConsideration>(true),
                Distance = GetComponentDataFromEntity<DistanceToConsideration>(true),
                entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer()
        }.Schedule(this, patrolAdd);

            JobHandle DetectionAdd = new AttackEnemyAdd()
            {
                targetBuffer = GetBufferFromEntity<TargetBuffer>(true),
                health = GetComponentDataFromEntity<HealthConsideration>(true),
                entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer()
            }.Schedule(this, waitAdd);

            JobHandle InvestigateAreaAddJob = new InvestigateAreaAdd()
            {
                health = GetComponentDataFromEntity<HealthConsideration>(true),
                Detect = GetComponentDataFromEntity<Detection>(true),
                DetectConsider =  GetComponentDataFromEntity<DetectionConsideration>(true),
                entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer()
            }.Schedule(this, DetectionAdd);


            // This is to be the last job of this system
            JobHandle ClearCreateTag = new ClearCreateTagJob()
            {
                entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer()
        }.Schedule(this, InvestigateAreaAddJob);
            return ClearCreateTag;
        }
        [BurstCompile]
        public struct PatrolAdd : IJobForEachWithEntity_EBCC<StateBuffer, Patrol, CreateAIBufferTag>
        {

            [ReadOnly] [NativeDisableParallelForRestriction] public ComponentDataFromEntity<HealthConsideration> health;
            [ReadOnly] [NativeDisableParallelForRestriction] public ComponentDataFromEntity<TimerConsideration> TimeC;// possible removing as it is not valid
            [ReadOnly] [NativeDisableParallelForRestriction] public ComponentDataFromEntity<DistanceToConsideration> Distance;

            [NativeDisableParallelForRestriction] public EntityCommandBuffer entityCommandBuffer;

            public void Execute(Entity entity, int Tindex, DynamicBuffer<StateBuffer> stateBuffer, [ReadOnly] ref Patrol c1, [ReadOnly]ref CreateAIBufferTag c2)
            {
                bool add = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.Patrol)
                    { add = false; }
                }

                if (add)
                {
                    stateBuffer.Add(new StateBuffer()
                    {
                        StateName = AIStates.Patrol,
                        Status = ActionStatus.Idle
                    });
                    if (!health.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent<HealthConsideration>(entity);

                    }
                    if (!TimeC.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent<TimerConsideration>(entity);

                    }
                    if (!Distance.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent<DistanceToConsideration>(entity);

                    }
                }
            }
        }


        [BurstCompile]
        public struct WaitAdd : IJobForEachWithEntity_EBCC<StateBuffer, WaitTime, CreateAIBufferTag>
        {

            [ReadOnly] [NativeDisableParallelForRestriction] public ComponentDataFromEntity<HealthConsideration> health;
            [ReadOnly] [NativeDisableParallelForRestriction] public ComponentDataFromEntity<TimerConsideration> TimeC;// possible removing as it is not valid
            [ReadOnly] [NativeDisableParallelForRestriction] public ComponentDataFromEntity<DistanceToConsideration> Distance;

            [NativeDisableParallelForRestriction] public EntityCommandBuffer entityCommandBuffer;

            public void Execute(Entity entity, int Tindex, DynamicBuffer<StateBuffer> stateBuffer, [ReadOnly] ref WaitTime c1, [ReadOnly]ref CreateAIBufferTag c2)
            {
                bool add = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.Wait)
                    { add = false; }
                }

                if (add)
                {
                    stateBuffer.Add(new StateBuffer()
                    {
                        StateName = AIStates.Wait,
                        Status = ActionStatus.Idle
                    });
                    if (!health.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent<HealthConsideration>(entity);

                    }
                    if (!TimeC.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent<TimerConsideration>(entity);

                    }
                    if (!Distance.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent<DistanceToConsideration>(entity);

                    }
                }
            }
        }
    }

    [BurstCompile]
    public struct AttackEnemyAdd : IJobForEachWithEntity_EBCC<StateBuffer, Detection, CreateAIBufferTag>
    {

        [ReadOnly] [NativeDisableParallelForRestriction] public BufferFromEntity<TargetBuffer> targetBuffer;
        [ReadOnly] [NativeDisableParallelForRestriction] public ComponentDataFromEntity<HealthConsideration> health;

        [NativeDisableParallelForRestriction] public EntityCommandBuffer entityCommandBuffer;

        public void Execute(Entity entity, int Tindex, DynamicBuffer<StateBuffer> stateBuffer, [ReadOnly] ref Detection c1, [ReadOnly]ref CreateAIBufferTag c2)
        {
            bool add = true;
            for (int index = 0; index < stateBuffer.Length; index++)
            {
                if (stateBuffer[index].StateName == AIStates.Attack_Melee)
                { add = false; }
            }

            if (add)
            {
                stateBuffer.Add(new StateBuffer()
                {
                    StateName = AIStates.Attack_Melee,
                    Status = ActionStatus.Idle
                });
                if (!targetBuffer.Exists(entity))
                {
                    entityCommandBuffer.AddBuffer<TargetBuffer>(entity);
                }
                if (!health.Exists(entity))
                {
                    entityCommandBuffer.AddComponent<HealthConsideration>(entity);

                }

            }
        }
    }

    public struct ClearCreateTagJob : IJobForEachWithEntity_EC<CreateAIBufferTag>
    {
        [NativeDisableParallelForRestriction] public EntityCommandBuffer entityCommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly]ref  CreateAIBufferTag Tag)
        {
            entityCommandBuffer.RemoveComponent<CreateAIBufferTag>(entity);
        }
    }
}
