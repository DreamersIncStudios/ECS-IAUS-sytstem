using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
namespace IAUS.ECS2
{
    [GenerateAuthoringComponent]
    public struct TargetBuffer : IBufferElementData
    {
        public Entity target;
    }

}