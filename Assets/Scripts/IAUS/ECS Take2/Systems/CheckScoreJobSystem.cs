using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

using IAUS.Core;
using Unity.Jobs;
using Utilities.ReactiveSystem;


[assembly: RegisterGenericComponentType(typeof(AIReactorSystem<IAUS.ECS2.BaseAI,  IAUS.ECS2.BaseAIReactor>.StateComponent))]


namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_UpdateState))]

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
                  

                  ////Update states when a state finishes based on states in Map
                  //if (AI.CurrentState.Status == ActionStatus.Success)
                  // {
                  //     Patrol Ptemp = PatrolFromEntity.Exists(entity) ? PatrolFromEntity[entity] : new Patrol();
                  //     WaitTime WTemp = Wait.Exists(entity) ? Wait[entity] : new WaitTime();
                  //    // FollowCharacter tempFollow = follow.Exists(entity) ? follow[entity] : new FollowCharacter();

                  //    for (int index = 0; index < State.Length; index++)
                  //     {
                  //         switch (State[index].StateName)
                  //         {
                  //             case AIStates.Patrol:
                  //                 switch (AI.CurrentState.StateName)
                  //                 {
                  //                     case AIStates.Wait:
                  //                         Ptemp.index++;
                  //                         if (Ptemp.index >= Ptemp.MaxNumWayPoint)
                  //                             Ptemp.index = 0;
                  //                         Ptemp.UpdatePostition = true;
                  //                         break;

                  //                     case AIStates.Rally:
                  //                         Ptemp.CanPatrol = true;
                  //                         Ptemp.Status = ActionStatus.Idle;

                  //                         break;
                  //                     case AIStates.Patrol:
                  //                         entityCommandBuffer.RemoveComponent<PatrolActionTag>(nativeThreadIndex, entity);

                  //                         break;
                  //                 }

                  //                 break;

                  //             case AIStates.Wait:
                  //                 switch (AI.CurrentState.StateName)
                  //                 {
                  //                     case AIStates.Patrol:
                  //                         WTemp.Timer = WTemp.TimeToWait;
                  //                         break;

                  //                     case AIStates.Wait:
                  //                         WTemp.Timer = 0.0f;
                  //                         entityCommandBuffer.RemoveComponent<WaitActionTag>(nativeThreadIndex, entity);
                  //                         break;

                  //                 }


                  //                 break;

                  //             case AIStates.GotoLeader:

                  //                //consider first updating all patrol points
                  //                switch (AI.CurrentState.StateName)
                  //                 {
                  //                     case AIStates.Patrol:

                  //                         break;
                  //                     case AIStates.Wait:
                  //                         break;
                  //                     case AIStates.GotoLeader:
                  //                         entityCommandBuffer.RemoveComponent<GetLeaderTag>(nativeThreadIndex, entity);

                  //                         break;
                  //                 }

                  //                 break;
                  //             case AIStates.Rally:

                  //                //consider first updating all patrol points
                  //                switch (AI.CurrentState.StateName)
                  //                 {
                  //                    // copy partol list of points from leader

                  //                    case AIStates.Patrol:

                  //                         break;
                  //                     case AIStates.Wait:
                  //                         break;
                  //                     case AIStates.Rally:
                  //                         entityCommandBuffer.RemoveComponent<RallyActionTag>(nativeThreadIndex, entity);

                  //                         break;
                  //                 }

                  //                 break;
                  //             case AIStates.FollowTarget:

                  //                //consider first updating all patrol points
                  //                switch (AI.CurrentState.StateName)
                  //                 {
                  //                    // copy partol list of points from leader

                  //                    case AIStates.Patrol:

                  //                         break;
                  //                     case AIStates.Wait:
                  //                         break;
                  //                     case AIStates.Rally:

                  //                         break;
                  //                     case AIStates.FollowTarget:
                  //                         entityCommandBuffer.RemoveComponent<FollowTargetTag>(nativeThreadIndex, entity);
                  //                         break;
                  //                 }

                  //                 break;
                  //         }
                  //     }

                  //     if (PatrolFromEntity.Exists(entity))
                  //         PatrolFromEntity[entity] = Ptemp;
                  //     if (Wait.Exists(entity))
                  //         Wait[entity] = WTemp;

                  // }

                  //// Rebalance Consider values for time wait;
                  //if (CheckState.StateName == AIStates.none)
                  //     return;


                  // if (CheckState.StateName != AI.CurrentState.StateName)
                  // {
                  //     switch (AI.CurrentState.StateName)
                  //     {
                  //        // Get compemenet set timer and status to completed
                  //        case AIStates.Patrol:
                  //             if (PatrolFromEntity.Exists(entity))
                  //             {
                  //                 entityCommandBuffer.RemoveComponent<PatrolActionTag>(nativeThreadIndex, entity);
                  //                 Patrol Ptemp = PatrolFromEntity[entity];
                  //                 if (Ptemp.Status == ActionStatus.Running)
                  //                 {
                  //                     Ptemp.Status = ActionStatus.Interrupted;
                  //                     Ptemp.ResetTime = Ptemp.ResetTimer / 2.0f;
                  //                 }
                  //                 PatrolFromEntity[entity] = Ptemp;
                  //             }
                  //             break;
                  //         case AIStates.Wait:
                  //             if (Wait.Exists(entity))
                  //             {
                  //                 entityCommandBuffer.RemoveComponent<WaitActionTag>(nativeThreadIndex, entity);
                  //                 WaitTime Wtemp = Wait[entity];
                  //                 if (Wtemp.Status == ActionStatus.Running)
                  //                 {
                  //                     Wtemp.Status = ActionStatus.Interrupted;
                  //                     Wtemp.ResetTime = Wtemp.ResetTimer / 2.0f;
                  //                 }
                  //                 Wait[entity] = Wtemp;
                  //             }
                  //             break;

                  //         case AIStates.GotoLeader:
                  //             if (party.Exists(entity))
                  //             {
                  //                 entityCommandBuffer.RemoveComponent<GetLeaderTag>(nativeThreadIndex, entity);

                  //                 Party tempParty = party[entity];
                  //                 if (tempParty.Status == ActionStatus.Running)
                  //                 {
                  //                     tempParty.Status = ActionStatus.Interrupted;
                  //                     tempParty.ResetTime = tempParty.ResetTimer / 2.0f;
                  //                 }
                  //                 party[entity] = tempParty;
                  //             }
                  //             break;
                  //         case AIStates.Rally:
                  //             if (rally.Exists(entity))
                  //             {
                  //                 entityCommandBuffer.RemoveComponent<RallyActionTag>(nativeThreadIndex, entity);

                  //                 Rally tempRally = rally[entity];
                  //                 if (tempRally.Status == ActionStatus.Running)
                  //                 {
                  //                     tempRally.Status = ActionStatus.Interrupted;
                  //                     tempRally.ResetTime = tempRally.ResetTimer / 2.0f;
                  //                 }
                  //                 rally[entity] = tempRally;
                  //             }
                  //             break;
                  //         case AIStates.FollowTarget:
                  //             if (follow.Exists(entity))
                  //             {
                  //                 entityCommandBuffer.RemoveComponent<FollowTargetTag>(nativeThreadIndex, entity);

                  //                 FollowCharacter tempFollow = follow[entity];
                  //                 if (tempFollow.Status == ActionStatus.Running)
                  //                 {
                  //                     tempFollow.Status = ActionStatus.Interrupted;
                  //                     tempFollow.ResetTime = tempFollow.ResetTimer / 2.0f;
                  //                 }
                  //                 follow[entity] = tempFollow;
                  //             }
                  //             break;
                  //     }




                  // }

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


        [UpdateInGroup(typeof(IAUS_UpdateState))]
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