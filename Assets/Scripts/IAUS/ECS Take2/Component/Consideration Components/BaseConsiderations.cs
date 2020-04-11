using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
namespace IAUS.ECS2
{
    public struct HealthConsideration : IComponentData
    {
        public float Ratio;
    }

    public struct DistanceToConsideration : IComponentData
    {
        public float Ratio;

    }
    public struct TimerConsideration : IComponentData
    {
        public float Ratio;
    }


}
