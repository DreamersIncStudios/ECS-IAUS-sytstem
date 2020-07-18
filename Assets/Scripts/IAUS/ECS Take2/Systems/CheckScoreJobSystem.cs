
using Unity.Entities;
using IAUS.Core;
using Unity.Jobs;
using Utilities.ReactiveSystem;


[assembly: RegisterGenericComponentType(typeof(AIReactorSystem<IAUS.ECS2.BaseAI,  IAUS.ECS2.BaseAIReactor>.StateComponent))]


namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_UpdateScore))]

    [UpdateBefore(typeof(StateScoreSystem))]
    public class CheckScoreJobSystem : SystemBase
    {
        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        protected override void OnUpdate()
        {

            ComponentDataFromEntity<Patrol> PatrolFromEntity = GetComponentDataFromEntity<Patrol>(false);
            ComponentDataFromEntity<WaitTime> Wait = GetComponentDataFromEntity<WaitTime>(false);
            ComponentDataFromEntity<Party> party = GetComponentDataFromEntity<Party>(false);
            ComponentDataFromEntity<Rally> rally = GetComponentDataFromEntity<Rally>(false);
            ComponentDataFromEntity<FollowCharacter> follow = GetComponentDataFromEntity<FollowCharacter>(false);

            EntityCommandBuffer.Concurrent entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();


            //   float DT = Time.DeltaTime;
            JobHandle systemDeps = Dependency;
            systemDeps = Entities.
              WithNativeDisableParallelForRestriction(PatrolFromEntity)
              .WithNativeDisableParallelForRestriction(Wait)
              .WithNativeDisableParallelForRestriction(party)
              .WithNativeDisableParallelForRestriction(rally)
              .WithNativeDisableParallelForRestriction(follow)
              .ForEach((Entity entity, int nativeThreadIndex, ref DynamicBuffer<StateBuffer> State, ref BaseAI AI) =>
              {
                  if (State.Length == 0)
                      return;
                  for (int index = 0; index < State.Length; index++)
                  {
                      StateBuffer Teststate = State[index];
                      switch (Teststate.StateName)
                      {
                          case AIStates.Wait:
                              WaitTime WTemp = Wait.Exists(entity) ? Wait[entity] : new WaitTime();
                              Teststate.TotalScore = WTemp.TotalScore;
                              Teststate.Status = WTemp.Status;
                              break;
                          case AIStates.Patrol:
                              Patrol PTemp = PatrolFromEntity.Exists(entity) ? PatrolFromEntity[entity] : new Patrol();
                              Teststate.TotalScore = PTemp.TotalScore;
                              Teststate.Status = PTemp.Status;
                              break;
                          case AIStates.GotoLeader:
                              Party tempParty = party.Exists(entity) ? party[entity] : new Party();
                              Teststate.TotalScore = tempParty.TotalScore;
                              Teststate.Status = tempParty.Status;
                              break;
                          case AIStates.Rally:
                              Rally tempRally = rally.Exists(entity) ? rally[entity] : new Rally();
                              Teststate.TotalScore = tempRally.TotalScore;
                              Teststate.Status = tempRally.Status;
                              break;
                          case AIStates.FollowTarget:
                              FollowCharacter tempFollow = follow.Exists(entity) ? follow[entity] : new FollowCharacter();
                              Teststate.TotalScore = tempFollow.TotalScore;
                              Teststate.Status = tempFollow.Status;
                              break;
                      }

                      if (State[index].StateName == AI.CurrentState.StateName)
                      {
                          AI.CurrentState = State[index];
                      }
                      State[index] = Teststate;
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
                case AIStates.Rally:
                    ECB.AddComponent<RallyActionTag>(ChunkID, entity);
                    break;
                case AIStates.FollowTarget:
                    ECB.AddComponent<FollowTargetTag>(ChunkID, entity);
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
                    case AIStates.Rally:
                        ECB.RemoveComponent<RallyActionTag>(ChunkID, entity);
                        break;
                    case AIStates.FollowTarget:
                        ECB.RemoveComponent<FollowTargetTag>(ChunkID, entity);
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
                case AIStates.Rally:
                    ECB.AddComponent<RallyActionTag>(ChunkID, entity);
                    break;
                case AIStates.FollowTarget:
                    ECB.AddComponent<FollowTargetTag>(ChunkID, entity);
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