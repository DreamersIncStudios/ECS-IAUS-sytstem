using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
namespace IAUS.Jobs
{
    public struct FindDistanceJob : IJobParallelFor
    {
        public NativeArray<Translation> PositionsCur;
        public NativeArray<Vector3> TargetPositions;
        public NativeArray<float> DistanceToTarget;
        public NativeArray<DistanceConsider> distanceConsiders;

        public void Execute(int index)
        {
            DistanceToTarget[index] = Vector3.Distance(PositionsCur[index].Value, TargetPositions[index]);
            DistanceConsider temp = distanceConsiders[index];
            temp.Score = distanceConsiders[index].Output(DistanceToTarget[index]);

            distanceConsiders[index]= temp;
        }
    }
}