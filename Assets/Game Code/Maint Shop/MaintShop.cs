using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DreamersIncStudio.Moonshoot
{
    public struct MaintShop : IComponentData
    {
        public float Range;
        public uint NumberOfWorkers;
        public uint NumberOfActiveWorkers;
    }

}