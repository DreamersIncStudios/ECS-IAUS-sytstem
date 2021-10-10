using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS.Component;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;
namespace IAUS.ECS.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]

    public sealed  class IAUSBrainUpdate : SystemBase
    {
        EntityCommandBufferSystem _entityCommandBufferSystem;
        public EntityQuery IAUSBrains;

        protected override void OnCreate()
        {
            base.OnCreate();
            
            IAUSBrains = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(IAUSBrain)), ComponentType.ReadWrite(typeof(StateBuffer)) },
               None = new ComponentType[] { ComponentType.ReadOnly(typeof(SetupBrainTag))}

            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        float interval = .20f;
        bool runUpdate => interval <= 0.0f;

        protected override void OnUpdate()
        {
            if (runUpdate)
            {
                JobHandle systemDeps = Dependency;
                systemDeps = new UpdateAIStateSchores()
                {
                    EntityChunk = GetEntityTypeHandle(),
                    StateChunks = GetBufferTypeHandle<StateBuffer>(false),
                    Patrols = GetComponentDataFromEntity<Patrol>(true),
                    Waits = GetComponentDataFromEntity<Wait>(true),
                    StayInRangeOfTarget = GetComponentDataFromEntity<StayInRange>(true),
                    GotoTarget = GetComponentDataFromEntity<MoveToTarget>(true),
                    RetreatCitizenScore = GetComponentDataFromEntity<RetreatCitizen>(true)
                    
                }.Schedule(IAUSBrains, systemDeps);
                _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

                systemDeps = new FindHighestScore()
                {
                    CommandBufferParallel = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                    EntityChunk = GetEntityTypeHandle(),
                    StateChunks = GetBufferTypeHandle<StateBuffer>(true),
                    BrainsChunk = GetComponentTypeHandle<IAUSBrain>(false)

                }.Schedule(IAUSBrains, systemDeps);
                _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
                //systemDeps.Complete();
                Dependency = systemDeps;
                interval = .20f;

            }
            else
            {
                interval -= 1 / 60.0f;
            }
        }

        [BurstCompile]
        public struct UpdateAIStateSchores : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public BufferTypeHandle<StateBuffer> StateChunks;

            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<Patrol> Patrols;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<Wait> Waits;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<StayInRange> StayInRangeOfTarget;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<MoveToTarget> GotoTarget;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<RetreatCitizen> RetreatCitizenScore;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Entity> Entities = chunk.GetNativeArray(EntityChunk);
                BufferAccessor<StateBuffer> Buffers = chunk.GetBufferAccessor(StateChunks);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity entity = Entities[i];
                    DynamicBuffer<StateBuffer> states = Buffers[i];

                    for (int j = 0; j < states.Length; j++)
                    {
                        StateBuffer temp = states[j];
                        switch (states[j].StateName)
                        {
                            case AIStates.Patrol:
                                temp.StateName = AIStates.Patrol;
                                temp.TotalScore = Patrols[entity].TotalScore;
                                temp.Status = Patrols[entity].Status;
                                break;
                            case AIStates.Wait:
                                temp.StateName = AIStates.Wait;
                                temp.TotalScore = Waits[entity].TotalScore;
                                temp.Status = Waits[entity].Status;
                                break;
                            case AIStates.ChaseMoveToTarget:
                                temp.StateName = AIStates.ChaseMoveToTarget;
                                temp.TotalScore = GotoTarget[entity].TotalScore;
                                temp.Status = GotoTarget[entity].Status;
                                break;
                            case AIStates.GotoLeader:
                                temp.StateName = AIStates.GotoLeader;
                                temp.TotalScore = StayInRangeOfTarget[entity].TotalScore;
                                temp.Status = StayInRangeOfTarget[entity].Status;
                                break;
                            case AIStates.Retreat:
                                temp.StateName = AIStates.Retreat;
                                if (RetreatCitizenScore.HasComponent(entity))
                                {
                                    temp.TotalScore = RetreatCitizenScore[entity].TotalScore;
                                    temp.Status = RetreatCitizenScore[entity].Status;
                                }
                                // add the other states later. 
                                    break;

                        }
                        states[j] = temp;

                    }
                }

            }
        }

        public struct FindHighestScore : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityChunk;
            [ReadOnly] public BufferTypeHandle<StateBuffer> StateChunks;
            public ComponentTypeHandle<IAUSBrain> BrainsChunk;
            public EntityCommandBuffer.ParallelWriter CommandBufferParallel;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Entity> Entities = chunk.GetNativeArray(EntityChunk);
                NativeArray<IAUSBrain> Brains = chunk.GetNativeArray(BrainsChunk);
                BufferAccessor<StateBuffer> Buffers = chunk.GetBufferAccessor(StateChunks);

                for (int i = 0; i < chunk.Count; i++)
                {
                    DynamicBuffer<StateBuffer> states = Buffers[i];
                    IAUSBrain brain = Brains[i];
                    StateBuffer tester =  new StateBuffer();
                    for (int j = 0; j < states.Length; j++)
                    {
                        if(tester.TotalScore < states[j].TotalScore && states[j].ConsiderScore)
                            tester = states[j];
                    }

                    if (tester.StateName != brain.CurrentState && tester.Status==ActionStatus.Idle)
                    {

                        switch (brain.CurrentState)
                        {
                            case AIStates.Patrol:
                                CommandBufferParallel.RemoveComponent<PatrolActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.Wait:
                                CommandBufferParallel.RemoveComponent<WaitActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.ChaseMoveToTarget:
                                CommandBufferParallel.RemoveComponent<MoveToTargetActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.GotoLeader:
                                CommandBufferParallel.RemoveComponent<StayInRangeActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.Attack:
                                //TODO Implement Add and Remove Tag;
                               // CommandBufferParallel.RemoveComponent<AttackTargetActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.Retreat:
                                CommandBufferParallel.RemoveComponent<RetreatActionTag>(chunkIndex, Entities[i]);
                                break;
                        }
                        //add new action tag
                        switch (tester.StateName)
                        {
                            case AIStates.Patrol:
                                CommandBufferParallel.AddComponent(chunkIndex, Entities[i], new PatrolActionTag() { UpdateWayPoint = false });
                                break;
                            case AIStates.Wait:
                                CommandBufferParallel.AddComponent<WaitActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.ChaseMoveToTarget:
                                CommandBufferParallel.AddComponent<MoveToTargetActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.GotoLeader:
                                CommandBufferParallel.AddComponent<StayInRangeActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.Attack:
                             //   CommandBufferParallel.AddComponent<AttackTargetActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.Retreat:
                                CommandBufferParallel.AddComponent<RetreatActionTag>(chunkIndex, Entities[i]);
                                break;

                        }
                        brain.CurrentState = tester.StateName;
                    }

                    Brains[i] = brain;
                }
            }
        }
    }
}