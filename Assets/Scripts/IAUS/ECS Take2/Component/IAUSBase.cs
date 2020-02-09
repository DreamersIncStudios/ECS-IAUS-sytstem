using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace IAUS.ECS2
{
    [GenerateAuthoringComponent]
    public struct IAUSBase : IComponentData
    {
        public AIStates CurrentStates;
    }
}