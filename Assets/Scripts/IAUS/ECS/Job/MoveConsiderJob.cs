using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Entities;
using IAUS.ECS.Consideration;
using IAUS.ECS.ComponentStates;
using ECS.Utilities;

namespace IAUS.ECS.Jobs
{[UpdateAfter(typeof (CopyTransformFromInjectedGameObjectECS))]
    public class MoveScoreJob : JobComponentSystem
    {
        struct Consider : IJobForEach<MoveToComponent, LocalToWorld>
        {
            public void Execute(ref MoveToComponent move, ref  LocalToWorld translation)
            {
                move.Distance = Vector3.Distance(translation.Position, move.Target);
                ConsiderationBaseECS temp = move.DistanceConsider;
                temp.Score = move.DistanceConsider.Output(move.Distance);

                move.DistanceConsider = temp;
                // Debug.Log(move.DS.Score);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new Consider() { };
            return job.Schedule(this, inputDeps);
        }
    }

}