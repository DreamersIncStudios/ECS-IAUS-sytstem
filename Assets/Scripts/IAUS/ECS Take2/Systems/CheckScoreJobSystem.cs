using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using IAUS.ECS.Component;

namespace IAUS.ECS2
{
    [UpdateBefore(typeof(StateScoreSystem))]
    public class CheckScoreJobSystem : ComponentSystem
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

            EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();


            float DT = Time.DeltaTime;
            Entities.ForEach((Entity entity, DynamicBuffer<StateBuffer> State, ref BaseAI AI) =>
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
                    if (AI.CurrentState.Status == ActionStatus.Success)
                    {
                        Patrol Ptemp = PatrolFromEntity.Exists(entity) ? PatrolFromEntity[entity] : new Patrol();
                        WaitTime WTemp = Wait.Exists(entity) ? Wait[entity] : new WaitTime();

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
                                    }
                                    entityCommandBuffer.RemoveComponent<PatrolActionTag>(entity);

                                    break;

                                case AIStates.Wait:
                                    switch (AI.CurrentState.StateName)
                                    {
                                        case AIStates.Patrol:
                                            WTemp.Timer = WTemp.TimeToWait;
                                            break;

                                        case AIStates.Wait:
                                            WTemp.Timer = 0.0f;
                                            break;

                                    }
                                    entityCommandBuffer.RemoveComponent<WaitActionTag>(entity);

                                    break;

                                case AIStates.GotoLeader:

                                    //consider first updating all patrol points
                                    switch (AI.CurrentState.StateName)
                                    {
                                        case AIStates.Patrol:

                                            break;
                                        case AIStates.Wait:
                                            break;
                                    }
                                    entityCommandBuffer.RemoveComponent<GetLeaderTag>(entity);

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
                                    entityCommandBuffer.RemoveComponent<PatrolActionTag>(entity);
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
                                    entityCommandBuffer.RemoveComponent<WaitActionTag>(entity);
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
                                    entityCommandBuffer.RemoveComponent<GetLeaderTag>(entity);

                                    Party tempParty = party[entity];
                                    if (tempParty.Status == ActionStatus.Running)
                                    {
                                        tempParty.Status = ActionStatus.Interrupted;
                                        tempParty.ResetTime = tempParty.ResetTimer / 2.0f;
                                    }
                                    party[entity] = tempParty;
                                }
                                break;
                        }

                        CheckState.Status = ActionStatus.Running;

                        AI.CurrentState = CheckState;

                        switch (AI.CurrentState.StateName)
                        {
                            // Get compemenet set timer and status to completed
                            case AIStates.Patrol:
                                entityCommandBuffer.AddComponent<PatrolActionTag>(entity);

                                break;
                            case AIStates.Wait:
                                entityCommandBuffer.AddComponent<WaitActionTag>(entity);
                                break;
                            case AIStates.GotoLeader:
                                entityCommandBuffer.AddComponent<GetLeaderTag>(entity);
                                break;
                        }

                    }

                });


        }
    }
}
