using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;

namespace IAUS.ECS2
{
    [UpdateAfter(typeof(StateScoreSystem))]
    public class WaitAction : JobComponentSystem
    {

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //EntityCommandBuffer entityCommandBuffer = entityCommandBufferSys.CreateCommandBuffer();
            float DT = Time.DeltaTime;
            JobHandle WaitAction = Entities.ForEach((Entity entity,ref WaitActionTag Wait, ref WaitTime Timer) => {
                //start
                if (Timer.Status != ActionStatus.Success)
                    return;

                if (Timer.Status != ActionStatus.Running)
                    Timer.Status = ActionStatus.Running;
                //Running
                if (Timer.Timer > 0.0f)
                    Timer.Timer -= DT;
            //complete
            if (Timer.Timer <= 0.0f) {
                    Timer.Status = ActionStatus.Success;
                    Timer.ResetTime = Timer.ResetTimer;
                    //Consider removing or let system do that 
                    //entityCommandBuffer.RemoveComponent<WaitActionTag>(entity);
                }
                



            }).Schedule(inputDeps);
            return WaitAction;
        }
    }


    public struct WaitActionTag : IComponentData { }
}