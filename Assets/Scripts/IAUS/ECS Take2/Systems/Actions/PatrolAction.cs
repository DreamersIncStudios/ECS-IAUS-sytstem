using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Jobs;
using IAUS.ECS.Component;

namespace IAUS.ECS2
{
    [UpdateAfter(typeof(StateScoreSystem))]
    public class PatrolAction : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float DT = Time.DeltaTime;
            JobHandle PatrolAction = Entities.ForEach((Entity entity, ref PatrolActionTag PatrolTag, ref Patrol patrol, ref Movement move, ref DynamicBuffer<PatrolBuffer> buffer, ref WaitTime wait, ref LocalToWorld toWorld) => {
                //start
                if (patrol.Status == ActionStatus.Success)
                    return;
                if (patrol.Status == ActionStatus.Idle) {
                    patrol.DistanceAtStart = Vector3.Distance(toWorld.Position, buffer[patrol.index].Point);
                }

                //Running
               if(!buffer[patrol.index].Point.Equals( move.TargetLocation)) 
                {
                    move.TargetLocation = buffer[patrol.index].Point;
                    patrol.Status = ActionStatus.Running;
                    move.Completed = false;
                    move.CanMove = true;
                }
                //complete

                if (move.Completed)
                {
                    patrol.Status = ActionStatus.Success;
                    patrol.ResetTime = patrol.ResetTimer;
                    wait.Timer = wait.TimeToWait;
                }

            }).Schedule(inputDeps);

            return PatrolAction;
        }
    }

    public struct PatrolActionTag : IComponentData { }
}