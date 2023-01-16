using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace DreamersInc.BestiarySystem
{
    public struct SpawnTag : IComponentData
    {
        public float3 SpawnPos;
        public uint ID;
        public uint Qty; 
    }
}