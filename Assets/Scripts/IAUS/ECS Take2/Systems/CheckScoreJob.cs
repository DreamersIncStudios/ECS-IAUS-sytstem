using Unity.Entities;
using Unity.Collections;
using IAUS.ECS.Component;
using Unity.Burst;
namespace IAUS.ECS2
{
    [BurstCompile]
    public struct CheckScores : IJobForEachWithEntity_EBC<StateBuffer, BaseAI>
    {
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<Patrol> Patrol;
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<WaitTime> Wait;
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<Movement> Move;

        [NativeDisableParallelForRestriction] public EntityCommandBuffer entityCommandBuffer;
        StateBuffer CheckState;

        public void Execute(Entity entity, int Tindex, DynamicBuffer<StateBuffer> State, ref BaseAI AI)
        {
            for (int index = 0; index < State.Length; index++)
            {
                StateBuffer Teststate = State[index];
                switch (Teststate.StateName)
                {
                    case AIStates.Wait:
                        WaitTime WTemp = Wait[entity];
                        Teststate.TotalScore = WTemp.TotalScore;
                        Teststate.Status = WTemp.Status;
                        break;
                    case AIStates.Patrol:
                        Patrol PTemp = Patrol[entity];
                        Teststate.TotalScore = PTemp.TotalScore;
                        Teststate.Status = PTemp.Status;
                        break;

                }
                State[index] = Teststate;
            }

            CheckState = new StateBuffer();


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
                Patrol Ptemp = Patrol[entity];
                WaitTime WTemp = Wait[entity];
                for (int index = 0; index < State.Length; index++)
                {
                    switch (State[index].StateName)
                    {
                        case AIStates.Patrol:
                            switch (AI.CurrentState.StateName)
                            {
                                case AIStates.Patrol:
                                    break;
                                case AIStates.Wait:
                                    Ptemp.index++;
                                    Ptemp.UpdatePostition = true;
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
                                    break;

                            }
                            break;
                    }
                }

                Patrol[entity] = Ptemp;
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
                        entityCommandBuffer.RemoveComponent<PatrolActionTag>(entity);
                        Patrol Ptemp = Patrol[entity];
                        Movement move = Move[entity];
                        //move.CanMove = false;
                        //move.Completed = false;
                        Move[entity] = move;
                        if (Ptemp.Status == ActionStatus.Running)
                        {
                            Ptemp.Status = ActionStatus.Interrupted;
                            Ptemp.ResetTime = Ptemp.ResetTimer / 2.0f;
                        }
                        Patrol[entity] = Ptemp;
                        break;
                    case AIStates.Wait:
                        entityCommandBuffer.RemoveComponent<WaitActionTag>(entity);
                        WaitTime Wtemp = Wait[entity];
                        if (Wtemp.Status == ActionStatus.Running)
                        {
                            Wtemp.Status = ActionStatus.Interrupted;
                            Wtemp.ResetTime = Wtemp.ResetTimer / 2.0f;
                        }
                        Wait[entity] = Wtemp;
                        break;
                }

                CheckState.Status = ActionStatus.Running;

                AI.CurrentState = CheckState;

                switch (AI.CurrentState.StateName)
                {
                    // Get compemenet set timer and status to completed
                    case AIStates.Patrol:
                        entityCommandBuffer.AddComponent(entity, new PatrolActionTag());

                        break;
                    case AIStates.Wait:
                        entityCommandBuffer.AddComponent(entity, new WaitActionTag());
                        break;
                }

            }
        }
    }

}