using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

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

                float mod = 1.0f - (1.0f / 3.0f);
                float TotalScore = patrol.Health.Output(health.Ratio) *
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

                float mod = 1.0f - (1.0f / 3.0f);
                float TotalScore = Wait.Health.Output(health.Ratio) *
                Wait.DistanceToTarget.Output(distanceTo.Ratio) *
                 Wait.WaitTimer.Output(timer.Ratio);

                Wait.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * mod) * TotalScore);
          
            }).Schedule(PatrolJob);
            WaitJob.Complete();
            var tester = new TestScore()
            {
                Patrol = GetComponentDataFromEntity<Patrol>(false),
                Wait = GetComponentDataFromEntity<WaitTime>(false),
                entityCommandBuffer = entityCommandBuffer.CreateCommandBuffer()
            }.Schedule(this, WaitJob);
            tester.Complete();
            return tester;
        }
    }
    [BurstCompile]
    public struct TestScore : IJobForEachWithEntity_EBC<StateBuffer,TestAI>
    {
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<Patrol> Patrol;
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<WaitTime> Wait;
        [NativeDisableParallelForRestriction] public EntityCommandBuffer entityCommandBuffer;
        StateBuffer CheckState;

        public void Execute(Entity entity, int Tindex, DynamicBuffer<StateBuffer> State, ref TestAI AI)
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
                if (State[index].Status == ActionStatus.Idle || State[index].Status == ActionStatus.Running)
                {
                    if (State[index].TotalScore > CheckState.TotalScore)
                        CheckState = State[index];
                }
            }
            //Debug.Log("Running");

            if (CheckState.StateName != AI.CurrentState.StateName)
            {
                switch (AI.CurrentState.StateName) {
                    // Get compemenet set timer and status to completed
                    case AIStates.Patrol:
                        entityCommandBuffer.RemoveComponent<PatrolActionTag>(entity);
                        Patrol Ptemp = Patrol[entity];
                        if (Ptemp.Status == ActionStatus.Running)
                            Ptemp.Status = ActionStatus.Interrupted;
                        Ptemp.ResetTime = Ptemp.ResetTimer;
                        break;
                    case AIStates.Wait:
                        entityCommandBuffer.RemoveComponent<WaitAction>(entity);
                        Patrol temp = Patrol[entity];
                        if (temp.Status == ActionStatus.Running)
                            temp.Status = ActionStatus.Interrupted;
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
