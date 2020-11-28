using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace DreamersStudio.TargetingSystem
{
    [GenerateAuthoringComponent]
    public struct LookAtTarget : IComponentData
    {
        public int BufferIndex;
        public bool LookatSomething;

        public void LookAt() { }
        public void Swap()
        {


        }
    }
}