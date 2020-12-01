using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
namespace IAUS.ECS2
{

    public struct DistanceToConsideration : IComponentData
    {
        public float Ratio;
        public float test;
    }

    public struct ThreatInAreaConsideration : IComponentData {
        public float Ratio;
    }

    public struct HealTimerConsideration : IComponentData { 
        public float Ratio;
        public float Timer;
    }
}
