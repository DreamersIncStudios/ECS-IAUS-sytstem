
using Unity.Entities;

using IAUS.Core;
using Unity.Jobs;

namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_UpdateState))]

    [UpdateBefore(typeof(StateScoreSystem))]
    public class CheckScoreJobSystem :JobComponentSystem
    {
        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            ComponentDataFromEntity<Patrol> PatrolFromEntity = GetComponentDataFromEntity<Patrol>(false);
            ComponentDataFromEntity<WaitTime> Wait = GetComponentDataFromEntity<WaitTime>(false);
            ComponentDataFromEntity<Party> party = GetComponentDataFromEntity<Party>(false);
            ComponentDataFromEntity<Rally> rally = GetComponentDataFromEntity<Rally>(false);
            ComponentDataFromEntity<FollowCharacter> follow = GetComponentDataFromEntity<FollowCharacter>(false);

            EntityCommandBuffer.Concurrent entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();


            float DT = Time.DeltaTime;
           JobHandle test = Entities.
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
                                Rally tempRally= rally.Exists(entity) ? rally[entity] : new Rally();
                                Teststate.TotalScore = tempRally.TotalScore;
                                Teststate.Status = tempRally.Status;
                                break;
                            case AIStates.FollowTarget:
                                FollowCharacter tempFollow= follow.Exists(entity) ? follow[entity] : new FollowCharacter();
                                Teststate.TotalScore = tempFollow.TotalScore;
                                Teststate.Status = tempFollow.Status;
                                break;
                        }
                        State[index] = Teststate;
                    }

                    StateBuffer CheckState = new StateBuffer();


                    for (int index = 0; index < State.Length; index++)
                    {
                        if (State[index].StateName == AI.CurrentState.StateName)
                            AI.CurrentState = State[index];
                        //  Debug.Log(AI.CurrentState.StateName + "is "+ AI.CurrentState.Status);
                        //move update to here;

                        if (State[index].Status == ActionStatus.Idle || State[index].Status == ActionStatus.Running)
                        {
                            if (State[index].TotalScore > CheckState.TotalScore)
                                CheckState = State[index];
                        }
                    }

                    //Update states when a state finishes based on states in Map
                    if (AI.CurrentState.Status == ActionStatus.Success )
                    {
                        Patrol Ptemp = PatrolFromEntity.Exists(entity) ? PatrolFromEntity[entity] : new Patrol();
                        WaitTime WTemp = Wait.Exists(entity) ? Wait[entity] : new WaitTime();
                       // FollowCharacter tempFollow = follow.Exists(entity) ? follow[entity] : new FollowCharacter();

                        for (int index = 0; index < State.Length; index++)
                        {
                            switch (State[index].StateName)
                            {
                                case AIStates.Patrol:
                                    switch (AI.CurrentState.StateName)
                                    {
                                        case AIStates.Wait:
                                            Ptemp.index++;
                                            if (Ptemp.index >= Ptemp.MaxNumWayPoint)
                                                Ptemp.index = 0;
                                            Ptemp.UpdatePostition = true;
                                            break;

                                        case AIStates.Rally:
                                            Ptemp.CanPatrol = true;
                                            Ptemp.Status = ActionStatus.Idle;

                                            break;
                                        case AIStates.Patrol:
                                            entityCommandBuffer.RemoveComponent<PatrolActionTag>(nativeThreadIndex,entity);

                                            break;
                                    }

                                    break;

                                case AIStates.Wait:
                                    switch (AI.CurrentState.StateName)
                                    {
                                        case AIStates.Patrol:
                                            WTemp.Timer = WTemp.TimeToWait;
                                            break;

                                        case AIStates.Wait:
                                            WTemp.Timer = 0.0f;
                                            entityCommandBuffer.RemoveComponent<WaitActionTag>(nativeThreadIndex,entity);
                                            break;

                                    }
                                 

                                    break;

                                case AIStates.GotoLeader:

                                    //consider first updating all patrol points
                                    switch (AI.CurrentState.StateName)
                                    {
                                        case AIStates.Patrol:

                                            break;
                                        case AIStates.Wait:
                                            break;
                                        case AIStates.GotoLeader:
                                            entityCommandBuffer.RemoveComponent<GetLeaderTag>(nativeThreadIndex,entity);

                                            break;
                                    }

                                    break;
                                case AIStates.Rally:

                                    //consider first updating all patrol points
                                    switch (AI.CurrentState.StateName)
                                    {
                                        // copy partol list of points from leader

                                        case AIStates.Patrol:

                                            break;
                                        case AIStates.Wait:
                                            break;
                                        case AIStates.Rally:
                                    entityCommandBuffer.RemoveComponent<RallyActionTag>(nativeThreadIndex,entity);

                                            break;
                                    }

                                    break;
                                case AIStates.FollowTarget:

                                    //consider first updating all patrol points
                                    switch (AI.CurrentState.StateName)
                                    {
                                        // copy partol list of points from leader

                                        case AIStates.Patrol:

                                            break;
                                        case AIStates.Wait:
                                            break;
                                        case AIStates.Rally:

                                            break;
                                        case AIStates.FollowTarget:
                                            entityCommandBuffer.RemoveComponent<FollowTargetTag>(nativeThreadIndex,entity);
                                            break;
                                    }

                                    break;
                            }
                        }

                        if (PatrolFromEntity.Exists(entity))
                            PatrolFromEntity[entity] = Ptemp;
                        if (Wait.Exists(entity))
                            Wait[entity] = WTemp;
                      
                    }

                    // Rebalance Consider values for time wait;
                    if (CheckState.StateName == AIStates.none)
                        return;


                    if (CheckState.StateName != AI.CurrentState.StateName)
                    {
                        switch (AI.CurrentState.StateName)
                        {
                            // Get compemenet set timer and status to completed
                            case AIStates.Patrol:
                                if (PatrolFromEntity.Exists(entity))
                                {
                                    entityCommandBuffer.RemoveComponent<PatrolActionTag>(nativeThreadIndex, entity);
                                    Patrol Ptemp = PatrolFromEntity[entity];
                                    if (Ptemp.Status == ActionStatus.Running)
                                    {
                                        Ptemp.Status = ActionStatus.Interrupted;
                                        Ptemp.ResetTime = Ptemp.ResetTimer / 2.0f;
                                    }
                                    PatrolFromEntity[entity] = Ptemp;
                                }
                                break;
                            case AIStates.Wait:
                                if (Wait.Exists(entity))
                                {
                                    entityCommandBuffer.RemoveComponent<WaitActionTag>(nativeThreadIndex, entity);
                                    WaitTime Wtemp = Wait[entity];
                                    if (Wtemp.Status == ActionStatus.Running)
                                    {
                                        Wtemp.Status = ActionStatus.Interrupted;
                                        Wtemp.ResetTime = Wtemp.ResetTimer / 2.0f;
                                    }
                                    Wait[entity] = Wtemp;
                                }
                                break;

                            case AIStates.GotoLeader:
                                if (party.Exists(entity))
                                {
                                    entityCommandBuffer.RemoveComponent<GetLeaderTag>(nativeThreadIndex,entity);

                                    Party tempParty = party[entity];
                                    if (tempParty.Status == ActionStatus.Running)
                                    {
                                        tempParty.Status = ActionStatus.Interrupted;
                                        tempParty.ResetTime = tempParty.ResetTimer / 2.0f;
                                    }
                                    party[entity] = tempParty;
                                }
                                break;
                            case AIStates.Rally:
                                if (rally.Exists(entity))
                                {
                                    entityCommandBuffer.RemoveComponent<RallyActionTag>(nativeThreadIndex, entity);

                                    Rally tempRally = rally[entity];
                                    if (tempRally.Status == ActionStatus.Running)
                                    {
                                        tempRally.Status = ActionStatus.Interrupted;
                                        tempRally.ResetTime = tempRally.ResetTimer / 2.0f;
                                    }
                                   rally[entity] = tempRally;
                                }
                                break;
                            case AIStates.FollowTarget:
                                if (follow.Exists(entity))
                                {
                                    entityCommandBuffer.RemoveComponent<FollowTargetTag>(nativeThreadIndex,entity);

                                    FollowCharacter tempFollow = follow[entity];
                                    if (tempFollow.Status == ActionStatus.Running)
                                    {
                                        tempFollow.Status = ActionStatus.Interrupted;
                                        tempFollow.ResetTime = tempFollow.ResetTimer / 2.0f;
                                    }
                                    follow[entity] = tempFollow;
                                }
                                break;
                        }

                        CheckState.Status = ActionStatus.Running;

                        AI.CurrentState = CheckState;

                        switch (AI.CurrentState.StateName)
                        {
                            // Get compemenet set timer and status to completed
                            case AIStates.Patrol:
                                entityCommandBuffer.AddComponent<PatrolActionTag>(nativeThreadIndex,entity);

                                break;
                            case AIStates.Wait:
                                entityCommandBuffer.AddComponent<WaitActionTag>(nativeThreadIndex,entity);
                                break;
                            case AIStates.GotoLeader:
                                entityCommandBuffer.AddComponent<GetLeaderTag>(nativeThreadIndex,entity);
                                break;
                            case AIStates.Rally:
                                entityCommandBuffer.AddComponent<RallyActionTag>(nativeThreadIndex,entity);
                                break;
                            case AIStates.FollowTarget:
                                entityCommandBuffer.AddComponent<FollowTargetTag>(nativeThreadIndex,entity);
                                break;
                        }

                    }

                }).Schedule(inputDeps)
            ;

            return test;
        }
    }
}
