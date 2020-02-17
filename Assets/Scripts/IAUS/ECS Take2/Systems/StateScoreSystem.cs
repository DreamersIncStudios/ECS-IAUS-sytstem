using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using IAUS.ECS.Component;
namespace IAUS.ECS2
{
    [UpdateAfter(typeof(ConsiderationSystem))]
    [UpdateBefore(typeof(InfluenceMap.TakeTwo))]
    public class StateScoreSystem : JobComponentSystem
    {
        EntityCommandBufferSystem entityCommandBuffer;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float DT = Time.DeltaTime;

            JobHandle PatrolJob = Entities.ForEach((ref Patrol patrol, ref HealthConsideration health, ref DistanceToConsideration distanceTo, ref TimerConsideration timer) =>
            {
                if (patrol.Status != ActionStatus.Running && patrol.ResetTime>0.0f )
                {
                    patrol.ResetTime -= DT;
                }
                if (patrol.ResetTime <= 0.0f && patrol.Status != ActionStatus.Idle)
                    patrol.Status = ActionStatus.Idle;
                float mod = 1.0f - (1.0f / 3.0f);
                float TotalScore = patrol.Health.Output(health.Ratio)*
                 patrol.DistanceToTarget.Output(distanceTo.Ratio) *
                 patrol.WaitTimer.Output(timer.Ratio);
                patrol.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * mod) * TotalScore);

            }).Schedule(inputDeps   );
            PatrolJob.Complete();
            JobHandle WaitJob = Entities.ForEach((ref WaitTime Wait, ref HealthConsideration health, ref DistanceToConsideration distanceTo, ref TimerConsideration timer) =>
            {
                if (Wait.Status != ActionStatus.Running && Wait.ResetTime > 0.0f)
                {
                    Wait.ResetTime -= DT;
                }

                if (Wait.ResetTime <= 0.0f && Wait.Status != ActionStatus.Idle)
                    Wait.Status = ActionStatus.Idle;
                float mod = 1.0f - (1.0f / 3.0f);
                float TotalScore = Wait.Health.Output(health.Ratio) *
                Wait.DistanceToTarget.Output(distanceTo.Ratio) *
                 Wait.WaitTimer.Output(timer.Ratio);
                Wait.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * mod) * TotalScore);
          
            }).Schedule(PatrolJob);
            WaitJob.Complete();

            var tester = new ScoreAIJob()
            {
                Patrol = GetComponentDataFromEntity<Patrol>(false),
                Wait = GetComponentDataFromEntity<WaitTime>(false),
                Move= GetComponentDataFromEntity<Movement>(false),
                entityCommandBuffer = entityCommandBuffer.CreateCommandBuffer()
            }.Schedule(this, WaitJob);
            tester.Complete();
            return tester;
        }
    }
  
    public struct ScoreAIJob : IJobForEachWithEntity_EBC<StateBuffer,AITag>
    {
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<Patrol> Patrol;
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<WaitTime> Wait;
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<Movement> Move;

        [NativeDisableParallelForRestriction] public EntityCommandBuffer entityCommandBuffer;
        StateBuffer CheckState;

        public void Execute(Entity entity, int Tindex, DynamicBuffer<StateBuffer> State, ref AITag AI)
        {
            for (int index = 0; index < State.Length; index++)
            {
                StateBuffer Teststate = State[index];
                switch (Teststate.StateName) {
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
            if (AI.CurrentState.Status == ActionStatus.Success) {
                Debug.Log(AI.CurrentState.StateName + " Success");
                Patrol Ptemp = Patrol[entity];
                WaitTime WTemp = Wait[entity];
                for (int index = 0; index < State.Length; index++)
                {
                    switch (State[index].StateName)
                    {
                        case AIStates.Patrol:
                            switch (AI.CurrentState.StateName) {
                                case AIStates.Patrol:
                                    Ptemp.ResetTime = Ptemp.ResetTimer;
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
                                    WTemp.ResetTime = WTemp.ResetTimer;
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
                switch (AI.CurrentState.StateName) {
                    // Get compemenet set timer and status to completed
                    case AIStates.Patrol:
                        entityCommandBuffer.RemoveComponent<PatrolActionTag>(entity);
                        Patrol Ptemp = Patrol[entity];
                        Movement move = Move[entity];
                        move.CanMove = false;
                        Move[entity] = move;
                        if (Ptemp.Status == ActionStatus.Running)
                            Ptemp.Status = ActionStatus.Interrupted;
                        Ptemp.ResetTime = Ptemp.ResetTimer/2.0f;
                        Patrol[entity] = Ptemp;
                        break;
                    case AIStates.Wait:
                        entityCommandBuffer.RemoveComponent<WaitActionTag>(entity);
                        WaitTime Wtemp = Wait[entity];
                        if (Wtemp.Status == ActionStatus.Running)
                            Wtemp.Status = ActionStatus.Interrupted;
                        Wtemp.ResetTime = Wtemp.ResetTimer / 2.0f;
                        Wait[entity] = Wtemp;
                        break;
                }
                
                CheckState.Status = ActionStatus.Running;
 
                AI.CurrentState = CheckState;

                switch (AI.CurrentState.StateName) {
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
