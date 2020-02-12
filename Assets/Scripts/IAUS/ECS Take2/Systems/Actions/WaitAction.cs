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
            float DT = Time.DeltaTime;
            JobHandle WaitAction = Entities.ForEach((ref WaitActionTag Wait, ref WaitTime Timer) => {
                if (Timer.Timer>0.0f)
                    Timer.Timer -= DT;
                }).Schedule(inputDeps);
            return WaitAction;
        }
    }


    public struct WaitActionTag : IComponentData { }
}