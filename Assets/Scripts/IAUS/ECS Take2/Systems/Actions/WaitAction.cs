using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using IAUS.Core;
namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_UpdateState))]

    [UpdateAfter(typeof(StateScoreSystem))]
    public class WaitAction : JobComponentSystem
    {

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //EntityCommandBuffer entityCommandBuffer = entityCommandBufferSys.CreateCommandBuffer();
            float DT = Time.DeltaTime;
            JobHandle WaitAction = Entities.ForEach((Entity entity,ref WaitActionTag Wait, ref WaitTime Timer, ref Patrol patrol) => {
                //start
                if (Timer.Status == ActionStatus.Success)
                    return;


                //Running
                if (Timer.Timer > 0.0f)
                {
                    Timer.TimerStarted = true; 
                    Timer.Timer -= DT;
                    Timer.Status = ActionStatus.Running;
                }

 
                    //complete
                    if (Timer.TimerStarted)
                {
                    if (Timer.Timer <= 0.0f)
                    {
                        Timer.Status = ActionStatus.Success;

                        Timer.TimerStarted = false;
                        
                    }
                }



            }).Schedule(inputDeps);
            return WaitAction;
        }
    }


    public struct WaitActionTag : IComponentData { }
}