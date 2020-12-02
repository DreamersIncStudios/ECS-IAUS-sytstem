using System.Collections;
using UnityEngine;
using Unity.Entities;
namespace IAUS.ECS2.Component
{
    [System.Serializable]
    public struct AITarget : IComponentData
    {
        public TargetType Type;
    }

    public enum TargetType { 
        None,Character, Location, Vehicle 
    }
}