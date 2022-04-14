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

    public sealed partial class IAUSBrainUpdate : SystemBase
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

        float interval = 0.0f;
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
                    Traverses = GetComponentDataFromEntity<Traverse>(true),
                    Waits = GetComponentDataFromEntity<Wait>(true),
                    StayInRangeOfTarget = GetComponentDataFromEntity<StayInRange>(true),
                    GotoTarget = GetComponentDataFromEntity<MoveToTarget>(true),
                    RetreatCitizenScore = GetComponentDataFromEntity<RetreatCitizen>(true),
                    AttackStateScore = GetComponentDataFromEntity<AttackTargetState>(true),
                    GatherStateScore = GetComponentDataFromEntity<GatherResourcesState> (true),
                    RepairStateScore = GetComponentDataFromEntity<RepairState>(true),
                    SpawnStateScore = GetComponentDataFromEntity<SpawnDefendersState>(true)
                    
                    
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

                systemDeps = new CheckTagSystem()
                {
                    EntityChunk = GetEntityTypeHandle(),
                    BrainChunk = GetComponentTypeHandle<IAUSBrain>(false),
                    Patrols = GetComponentDataFromEntity<Patrol>(true),
                    Traverses = GetComponentDataFromEntity<Traverse>(true),
                    Waits = GetComponentDataFromEntity<Wait>(true),
                    StayInRangeOfTarget = GetComponentDataFromEntity<StayInRange>(true),
                    GotoTarget = GetComponentDataFromEntity<MoveToTarget>(true),
                    RetreatCitizenScore = GetComponentDataFromEntity<RetreatCitizen>(true),
                    AttackStateScore = GetComponentDataFromEntity<AttackTargetState>(true),
                    GatherStateScore = GetComponentDataFromEntity<GatherResourcesState>(true),
                    RepairStateScore = GetComponentDataFromEntity<RepairState>(true),
                    SpawnStateScore = GetComponentDataFromEntity<SpawnDefendersState>(true)


                }.Schedule(IAUSBrains, systemDeps);
                _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);


                //systemDeps.Complete();
                Dependency = systemDeps;
                interval = 1.0f;

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
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<Traverse> Traverses;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<Wait> Waits;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<StayInRange> StayInRangeOfTarget;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<MoveToTarget> GotoTarget;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<RetreatCitizen> RetreatCitizenScore;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<AttackTargetState> AttackStateScore;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<GatherResourcesState> GatherStateScore;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<RepairState> RepairStateScore;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<SpawnDefendersState> SpawnStateScore;

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
                            case AIStates.Traverse:
                                temp.StateName = AIStates.Traverse;
                                temp.TotalScore = Traverses[entity].TotalScore;
                                temp.Status = Traverses[entity].Status;
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
                            case AIStates.Attack:
                                temp.StateName = AIStates.Attack;
                                temp.TotalScore = AttackStateScore[entity].TotalScore;
                                temp.Status = AttackStateScore[entity].Status;
                                break;
                            case AIStates.GatherResources:
                                temp.StateName = AIStates.GatherResources;
                                if (GatherStateScore.HasComponent(entity))
                                {
                                    temp.TotalScore = GatherStateScore[entity].TotalScore;
                                    temp.Status = GatherStateScore[entity].Status;
                                }
                                break;
                            case AIStates.Heal_Magic:
                                temp.StateName = AIStates.Heal_Magic;
                                if (RepairStateScore.HasComponent(entity)) {
                                    temp.TotalScore = RepairStateScore[entity].TotalScore;
                                    temp.Status = RepairStateScore[entity].Status;
                                }

                                break;
                            case AIStates.CallBackUp:
                                temp.StateName = AIStates.CallBackUp;
                                if (SpawnStateScore.HasComponent(entity))
                                {
                                    temp.TotalScore = SpawnStateScore[entity].TotalScore;
                                    temp.Status = SpawnStateScore[entity].Status;
                                }

                                break;
                        }
                        states[j] = temp;

                    }
                }

            }
        }

        [BurstCompile]
        public struct CheckTagSystem : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public ComponentTypeHandle<IAUSBrain> BrainChunk;

            [NativeDisableParallelForRestriction][ReadOnly] public ComponentDataFromEntity<Patrol> Patrols;
            [NativeDisableParallelForRestriction][ReadOnly] public ComponentDataFromEntity<Traverse> Traverses;
            [NativeDisableParallelForRestriction][ReadOnly] public ComponentDataFromEntity<Wait> Waits;
            [NativeDisableParallelForRestriction][ReadOnly] public ComponentDataFromEntity<StayInRange> StayInRangeOfTarget;
            [NativeDisableParallelForRestriction][ReadOnly] public ComponentDataFromEntity<MoveToTarget> GotoTarget;
            [NativeDisableParallelForRestriction][ReadOnly] public ComponentDataFromEntity<RetreatCitizen> RetreatCitizenScore;
            [NativeDisableParallelForRestriction][ReadOnly] public ComponentDataFromEntity<AttackTargetState> AttackStateScore;
            [NativeDisableParallelForRestriction][ReadOnly] public ComponentDataFromEntity<GatherResourcesState> GatherStateScore;
            [NativeDisableParallelForRestriction][ReadOnly] public ComponentDataFromEntity<RepairState> RepairStateScore;
            [NativeDisableParallelForRestriction][ReadOnly] public ComponentDataFromEntity<SpawnDefendersState> SpawnStateScore;
            public EntityCommandBuffer.ParallelWriter CommandBufferParallel;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Entity> Entities = chunk.GetNativeArray(EntityChunk);
               NativeArray<IAUSBrain> Brains = chunk.GetNativeArray(BrainChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity entity = Entities[i];
                   IAUSBrain brain = Brains[i];

                    switch(brain.CurrentState)
                    {
                        
                           case AIStates.Patrol:
                            if(!Patrols.HasComponent(entity))
                                CommandBufferParallel.AddComponent(chunkIndex, Entities[i], new PatrolActionTag() { UpdateWayPoint = false });
                                break;
                            case AIStates.Traverse:
                            if (!Traverses.HasComponent(entity))
                                CommandBufferParallel.AddComponent(chunkIndex, Entities[i], new TraverseActionTag() { UpdateWayPoint = false });
                                break;
                            case AIStates.Wait:
                            if (!Waits.HasComponent(entity))
                                CommandBufferParallel.AddComponent<WaitActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.ChaseMoveToTarget:
                            if (!GotoTarget.HasComponent(entity))
                                CommandBufferParallel.AddComponent<MoveToTargetActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.GotoLeader:
                            if (!StayInRangeOfTarget.HasComponent(entity))
                                CommandBufferParallel.AddComponent<StayInRangeActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.Attack:
                            if (!AttackStateScore.HasComponent(entity))
                                CommandBufferParallel.AddComponent<AttackActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.Retreat:
                            if (!RetreatCitizenScore.HasComponent(entity))
                                CommandBufferParallel.AddComponent<RetreatActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.GatherResources:
                            if (!GatherStateScore.HasComponent(entity))
                                CommandBufferParallel.AddComponent<GatherResourcesTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.Heal_Magic:
                            if (!RepairStateScore.HasComponent(entity))
                                CommandBufferParallel.AddComponent<HealSelfTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.CallBackUp:
                            if (!SpawnStateScore .HasComponent(entity))
                                CommandBufferParallel.AddComponent<SpawnTag>(chunkIndex, Entities[i]);
                                break;

                       
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
                            case AIStates.Traverse:
                                CommandBufferParallel.RemoveComponent<TraverseActionTag>(chunkIndex, Entities[i]);
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
                                CommandBufferParallel.RemoveComponent<AttackActionTag>(chunkIndex, Entities[i]);
                                break;

                            case AIStates.Retreat:
                                CommandBufferParallel.RemoveComponent<RetreatActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.GatherResources:
                                CommandBufferParallel.RemoveComponent<GatherResourcesTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.Heal_Magic:
                                CommandBufferParallel.RemoveComponent<HealSelfTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.CallBackUp:
                                CommandBufferParallel.RemoveComponent<SpawnTag>(chunkIndex, Entities[i]);
                                break;
                        }
                        //add new action tag
                        switch (tester.StateName)
                        {
                            case AIStates.Patrol:
                                CommandBufferParallel.AddComponent(chunkIndex, Entities[i], new PatrolActionTag() { UpdateWayPoint = false });
                                break;
                            case AIStates.Traverse:
                                CommandBufferParallel.AddComponent(chunkIndex, Entities[i], new TraverseActionTag() { UpdateWayPoint = false });
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
                                CommandBufferParallel.AddComponent<AttackActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.Retreat:
                                CommandBufferParallel.AddComponent<RetreatActionTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.GatherResources:
                                CommandBufferParallel.AddComponent<GatherResourcesTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.Heal_Magic:
                                CommandBufferParallel.AddComponent<HealSelfTag>(chunkIndex, Entities[i]);
                                break;
                            case AIStates.CallBackUp:
                                CommandBufferParallel.AddComponent<SpawnTag>(chunkIndex, Entities[i]);
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