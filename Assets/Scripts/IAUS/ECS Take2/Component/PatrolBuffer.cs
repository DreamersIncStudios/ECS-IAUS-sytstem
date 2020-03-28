using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
namespace IAUS.ECS2
{
    [GenerateAuthoringComponent]
    public struct PatrolBuffer : IBufferElementData {

        public PatrolPoint WayPoint;
        public static implicit operator PatrolPoint(PatrolBuffer e) { return e; }
        public static implicit operator PatrolBuffer(PatrolPoint e) { return new PatrolBuffer { WayPoint = e }; }
        
    }
    
    [System.Serializable]
    public struct PatrolPoint {
        public float3 Point;
        public float WaitTime;
    }
}
