using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
namespace IAUS.ECS2
{
    [GenerateAuthoringComponent]
    public struct PatrolBuffer : IBufferElementData {
   
            public float3 Point;

            public static implicit operator float3(PatrolBuffer e) { return e; }
            public static implicit operator PatrolBuffer(float3 e) { return new PatrolBuffer { Point = e }; }
        
    }

}
