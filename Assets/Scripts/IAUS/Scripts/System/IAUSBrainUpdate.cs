﻿using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS2.Component;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;
namespace IAUS.ECS2.Systems
{
    public class IAUSBrainUpdate : SystemBase
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

        protected override void OnUpdate()
        {
            if (UnityEngine.Time.frameCount % 120 == 5)
            {
                JobHandle systemDeps = Dependency;
                systemDeps = new UpdateBrains()
                {
                    EntityChunk = GetArchetypeChunkEntityType(),
                    StateChunks = GetArchetypeChunkBufferType<StateBuffer>(false),
                    Patrols = GetComponentDataFromEntity<Patrol>(true),
                    Waits = GetComponentDataFromEntity<Wait>(true),
                    StayInRangeOfTarget = GetComponentDataFromEntity<StayInRange>(true),
                    GotoTarget = GetComponentDataFromEntity<MoveToTarget>(true),
                    MeleeAttackState = GetComponentDataFromEntity<MeleeAttackTarget>(true)
                    
                }.Schedule(IAUSBrains, systemDeps);
                _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

                systemDeps = new FindHighestScore()
                {
                    ConcurrentBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                    EntityChunk = GetArchetypeChunkEntityType(),
                    StateChunks = GetArchetypeChunkBufferType<StateBuffer>(true),
                    BrainsChunk = GetArchetypeChunkComponentType<IAUSBrain>(false)

                }.Schedule(IAUSBrains, systemDeps);
                _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
                //systemDeps.Complete();
                Dependency = systemDeps;

            }
        }

       [BurstCompile]
        public struct UpdateBrains : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
            public ArchetypeChunkBufferType<StateBuffer> StateChunks;

            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<Patrol> Patrols;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<Wait> Waits;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<StayInRange> StayInRangeOfTarget;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<MoveToTarget> GotoTarget;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<MeleeAttackTarget> MeleeAttackState;

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
                            case AIStates.Attack_Melee:
                                temp.StateName = AIStates.Attack_Melee;
                                temp.TotalScore = MeleeAttackState[entity].TotalScore;
                                temp.Status = MeleeAttackState[entity].Status;
                                break;

                        }
                        states[j] = temp;

                    }
                }

            }
        }

        public struct FindHighestScore : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
            [ReadOnly] public ArchetypeChunkBufferType<StateBuffer> StateChunks;
            public ArchetypeChunkComponentType<IAUSBrain> BrainsChunk;
            public EntityCommandBuffer.Concurrent ConcurrentBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Entity> Entities = chunk.GetNativeArray(EntityChunk);
                NativeArray<IAUSBrain> Brains = chunk.GetNativeArray(BrainsChunk);
                BufferAccessor<StateBuffer> Buffers = chunk.GetBufferAccessor(StateChunks);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity entity = Entities[i];
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
                                ConcurrentBuffer.RemoveComponent<PatrolActionTag>(chunkIndex, entity);
                                break;
                            case AIStates.Wait:
                                ConcurrentBuffer.RemoveComponent<WaitActionTag>(chunkIndex, entity);
                                break;
                            case AIStates.ChaseMoveToTarget:
                                ConcurrentBuffer.RemoveComponent<MoveToTargetActionTag>(chunkIndex, entity);
                                break;
                            case AIStates.GotoLeader:
                                ConcurrentBuffer.RemoveComponent<StayInRangeActionTag>(chunkIndex, entity);
                                break;
                            case AIStates.Attack_Melee:
                                ConcurrentBuffer.RemoveComponent<AttackTargetActionTag>(chunkIndex, entity);
                                break;
                        }
                        //add new action tag
                        switch (tester.StateName)
                        {
                            case AIStates.Patrol:
                                //if (brain.CurrentState==AIStates.Wait) {
                                //    ConcurrentBuffer.AddComponent(chunkIndex, entity, new PatrolActionTag() { UpdateWayPoint = true });
                                //} else
                                ConcurrentBuffer.AddComponent(chunkIndex, entity, new PatrolActionTag() { UpdateWayPoint = false });
                                break;
                            case AIStates.Wait:
                                ConcurrentBuffer.AddComponent<WaitActionTag>(chunkIndex, entity);
                                break;
                            case AIStates.ChaseMoveToTarget:
                                ConcurrentBuffer.AddComponent<MoveToTargetActionTag>(chunkIndex, entity);
                                break;
                            case AIStates.GotoLeader:
                                ConcurrentBuffer.AddComponent<StayInRangeActionTag>(chunkIndex, entity);
                                break;
                            case AIStates.Attack_Melee:
                                ConcurrentBuffer.AddComponent<AttackTargetActionTag>(chunkIndex, entity);
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