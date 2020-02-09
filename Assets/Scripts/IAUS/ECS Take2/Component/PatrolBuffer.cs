using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
namespace IAUS.ECS2
{
    public struct PatrolBuffer : IBufferElementData {
   
            public float3 Point;

            public static implicit operator float3(PatrolBuffer e) { return e.Point; }
            public static implicit operator PatrolBuffer(int e) { return new PatrolBuffer { Point = e }; }

        
    }

}
