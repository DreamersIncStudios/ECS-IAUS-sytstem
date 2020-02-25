using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
namespace IAUS.ECS2
{
    [GenerateAuthoringComponent]
    public struct TargetBuffer : IBufferElementData
    {
        public Target TargetToLookFor;
    }
    [System.Serializable]
    public struct Target {
        public Entity target;
        public RaycastCommand RaycastCom;
    }
}