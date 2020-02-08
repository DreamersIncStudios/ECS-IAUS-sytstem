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
        
        public Influence Player;
        public Influence Enemy;
        public Influence Global;

        public Vector3 Position;
        public float dist;
    }
}