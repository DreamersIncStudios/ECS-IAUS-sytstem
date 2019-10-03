using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Entities;
using IAUS.ECS.Consideration;
using IAUS.ECS.ComponentStates;

namespace IAUS.ECS.Jobs
{
    [UpdateAfter(typeof(WaitForScoreJob))]
    public class ScoreJob : JobComponentSystem
    {

        struct Score : IJobForEach<MoveToComponent, WaitForComponent>
        {

            public void Execute(ref MoveToComponent Move, ref WaitForComponent Wait)
            {
                Move.Score = Move.DistanceConsider.Output(Move.input(Move.Distance));// * Move.WaitForConsider.Output(Wait.Timer);
                Wait.Score = Wait.DistanceConsider.Output(Wait.input(Move.Distance));// * Wait.WaitForConsider.Output(Wait.Timer);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new Score() { };
            return job.Schedule(this, inputDeps);
        }
    }
}