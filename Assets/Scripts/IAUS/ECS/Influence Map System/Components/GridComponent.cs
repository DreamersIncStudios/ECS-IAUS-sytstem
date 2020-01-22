using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace InfluenceMap
{
    public struct GridComponent : IComponentData
    {
        public float width, height;
        public Vector2 cellsize;
        

    }
    public struct Gridpoint : IBufferElementData {
        
        public float TotalValue;
        public Threat threat;
        public Threat protection;
        public Vector3 Position;
    }
}