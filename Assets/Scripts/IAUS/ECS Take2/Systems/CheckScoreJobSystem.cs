
using Unity.Entities;
using IAUS.Core;
using Unity.Jobs;
using Utilities.ReactiveSystem;
using Unity.Collections;
using Unity.Burst;
using Dreamers.InventorySystem;

[assembly: RegisterGenericComponentType(typeof(AIReactorSystem<IAUS.ECS2.BaseAI,  IAUS.ECS2.BaseAIReactor>.StateComponent))]


namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_UpdateScore))]

    [UpdateBefore(typeof(StateScoreSystem))]
    public class CheckScoreJobSystem : SystemBase
    {
        EntityCommandBufferSystem entityCommandBufferSystem;
        EntityQuery patroller;
        EntityQuery Waiting;
        EntityQuery SelfHealers;
        EntityQuery Partyer;
        EntityQuery Retreat;
        EntityQuery Followers;
        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            patroller = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Patrol)), ComponentType.ReadWrite(typeof(StateBuffer)) }
            });
            Waiting = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(WaitTime)), ComponentType.ReadWrite(typeof(StateBuffer)) }
            });
           SelfHealers = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(HealSelfViaItem)), ComponentType.ReadWrite(typeof(StateBuffer)) }
            });
            Partyer = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Party)), ComponentType.ReadWrite(typeof(StateBuffer)) }
            });
            Retreat = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Retreat)), ComponentType.ReadWrite(typeof(StateBuffer)) }
            });
            Followers = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(FollowCharacter)), ComponentType.ReadWrite(typeof(StateBuffer)) }
            });
        }
        protected override void OnUpdate()
        {



            EntityCommandBuffer.Concurrent entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();


            //   float DT = Time.DeltaTime;
            JobHandle systemDeps = Dependency;
            systemDeps = new AISTATESCOREJOB<Patrol>() {
                AIStateChunk = GetArchetypeChunkComponentType<Patrol>(),
                StateBufferChunk = GetArchetypeChunkBufferType<StateBuffer>(),
                StateToUpdate = AIStates.Patrol
            
            }.ScheduleParallel(patroller, systemDeps);

            systemDeps = new AISTATESCOREJOB<WaitTime>()
            {
                AIStateChunk = GetArchetypeChunkComponentType<WaitTime>(),
                StateBufferChunk = GetArchetypeChunkBufferType<StateBuffer>(),
                StateToUpdate = AIStates.Wait

            }.ScheduleParallel(Waiting, systemDeps);
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AISTATESCOREJOB<HealSelfViaItem>()
            {
                AIStateChunk = GetArchetypeChunkComponentType<HealSelfViaItem>(),
                StateBufferChunk = GetArchetypeChunkBufferType<StateBuffer>(),
                StateToUpdate = AIStates.Heal_Self_Item

            }.ScheduleParallel(SelfHealers, systemDeps); 
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
     
            systemDeps = new AISTATESCOREJOB<Party>()
            {
                AIStateChunk = GetArchetypeChunkComponentType<Party>(),
                StateBufferChunk = GetArchetypeChunkBufferType<StateBuffer>(),
                StateToUpdate = AIStates.GotoLeader

            }.ScheduleParallel(Partyer, systemDeps);
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AISTATESCOREJOB<RetreatToLocation>()
            {
                AIStateChunk = GetArchetypeChunkComponentType<RetreatToLocation>(),
                StateBufferChunk = GetArchetypeChunkBufferType<StateBuffer>(),
                StateToUpdate = AIStates.RetreatToLocation

            }.ScheduleParallel(Retreat, systemDeps);
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AISTATESCOREJOB<FollowCharacter>()
            {
                AIStateChunk = GetArchetypeChunkComponentType<FollowCharacter>(),
                StateBufferChunk = GetArchetypeChunkBufferType<StateBuffer>(),
                StateToUpdate = AIStates.FollowTarget

            }.ScheduleParallel(Followers, systemDeps);
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);


            systemDeps = Entities.ForEach((Entity entity, int nativeThreadIndex, ref DynamicBuffer<StateBuffer> State, ref BaseAI AI) =>
              {
                  if (State.Length == 0)
                      return;
                  for (int index = 0; index < State.Length; index++)
                  {
                   

                      if (State[index].StateName == AI.CurrentState.StateName)
                      {
                          AI.CurrentState = State[index];
                      }
                   
                  }

                  StateBuffer CheckState = new StateBuffer();

                  for (int index = 0; index < State.Length; index++)
                  {
                      if (State[index].Status == ActionStatus.Idle || State[index].Status == ActionStatus.Running)
                      {
                          if (State[index].TotalScore > CheckState.TotalScore)
                          {
                              CheckState = State[index];
                          }
                      }
                  }
                  if (CheckState.TotalScore == 0.0f)
                      return;

                  if (CheckState.StateName != AI.CurrentState.StateName )
                  {
                      CheckState.Status = ActionStatus.Running;
                      AI.CurrentState = CheckState;

                      AI.Set = true;
                  }
                  


              }).ScheduleParallel(systemDeps);
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }


        [BurstCompile]
    struct AISTATESCOREJOB<AISTATE> : IJobChunk
            where AISTATE: unmanaged, BaseStateScorer
        {
            public ArchetypeChunkComponentType<AISTATE> AIStateChunk;
            public ArchetypeChunkBufferType<StateBuffer> StateBufferChunk;
            public AIStates StateToUpdate;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<AISTATE> AISTATES = chunk.GetNativeArray(AIStateChunk);
                BufferAccessor<StateBuffer> accessor = chunk.GetBufferAccessor(StateBufferChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    AISTATE aistate = AISTATES[i];
                    DynamicBuffer<StateBuffer> State = accessor[i];

                    if (State.Length == 0)
                        return;
                    for (int index = 0; index < State.Length; index++)
                    {
                        StateBuffer Teststate = State[index];
                        if (Teststate.StateName == StateToUpdate)
                        {
                            Teststate.TotalScore = aistate.TotalScore;
                            Teststate.Status = aistate.Status;
                        }      
                        State[index] = Teststate;
                    }
                }
            }
        }
     


    }
    public struct BaseAIReactor : AIComponentReactor<BaseAI>
    {
        public void ComponentAdded(Entity entity, ref BaseAI newComponent, ref EntityCommandBuffer.Concurrent ECB, ref int ChunkID)
        {
            if (!newComponent.Set)
                return;
            
            newComponent.CurrentState.Status = ActionStatus.Running;
            switch (newComponent.CurrentState.StateName)
            {
                // Get compemenet set timer and status to completed
                case AIStates.Patrol:
                    ECB.AddComponent<PatrolActionTag>(ChunkID, entity);

                    break;
                case AIStates.Wait:
                    ECB.AddComponent<WaitActionTag>(ChunkID, entity);
                    break;
                case AIStates.GotoLeader:
                    ECB.AddComponent<GetLeaderTag>(ChunkID, entity);
                    break;
                case AIStates.RetreatToLocation:
                    ECB.AddComponent<RallyActionTag>(ChunkID, entity);
                    break;
                case AIStates.FollowTarget:
                    ECB.AddComponent<FollowTargetTag>(ChunkID, entity);
                    break;
                case AIStates.Heal_Self_Item:
                    ECB.AddComponent<HealSelfActionTag>(ChunkID, entity);
                    break;
            }
            
        }

        public void ComponentValueChanged(ref Entity entity, ref BaseAI newComponent, in BaseAI oldComponent, ref EntityCommandBuffer.Concurrent ECB, ref int ChunkID)
        {
            if (!newComponent.Set)
                return;

            if (oldComponent.CurrentState.Status == ActionStatus.Success)
            {
                switch (oldComponent.CurrentState.StateName)
                {
                    case AIStates.Patrol:
                        ECB.RemoveComponent<PatrolActionTag>(ChunkID, entity);

                        break;
                    case AIStates.Wait:
                        ECB.RemoveComponent<WaitActionTag>(ChunkID, entity);
                        break;
                    case AIStates.GotoLeader:
                        ECB.RemoveComponent<GetLeaderTag>(ChunkID, entity);
                        break;
                    case AIStates.RetreatToLocation:
                        ECB.RemoveComponent<RallyActionTag>(ChunkID, entity);
                        break;
                    case AIStates.FollowTarget:
                        ECB.RemoveComponent<FollowTargetTag>(ChunkID, entity);
                        break;
                    case AIStates.Heal_Self_Item:
                        ECB.RemoveComponent<HealSelfActionTag>(ChunkID, entity);
                        break;
                }
            }
            newComponent.CurrentState.Status = ActionStatus.Running;
            switch (newComponent.CurrentState.StateName)
            {
                // Get compemenet set timer and status to completed
                case AIStates.Patrol:
                    ECB.AddComponent<PatrolActionTag>(ChunkID, entity);

                    break;
                case AIStates.Wait:
                    ECB.AddComponent<WaitActionTag>(ChunkID, entity);
                    break;
                case AIStates.GotoLeader:
                    ECB.AddComponent<GetLeaderTag>(ChunkID, entity);
                    break;
                case AIStates.RetreatToLocation:
                    ECB.AddComponent<RallyActionTag>(ChunkID, entity);
                    break;
                case AIStates.FollowTarget:
                    ECB.AddComponent<FollowTargetTag>(ChunkID, entity);
                    break;
                case AIStates.Heal_Self_Item:
                    ECB.AddComponent<HealSelfActionTag>(ChunkID, entity);
                    break;
            }
            newComponent.Set = false;

        }


        [UpdateInGroup(typeof(IAUS_UpdateScore))]
        [UpdateBefore(typeof(StateScoreSystem))]
        [UpdateAfter(typeof(CheckScoreJobSystem))]
        public class AIReactionSystem : AIReactorSystem<BaseAI, BaseAIReactor>
        {
            protected override BaseAIReactor CreateComponentRactor()
            {
                return new BaseAIReactor();
            }
        }
    }

   
}