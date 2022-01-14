using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using DreamersInc.ComboSystem;

namespace IAUS.ECS.Component
{
    [System.Serializable]
    [GenerateAuthoringComponent]
    public struct NPCAttackBuffer : IBufferElementData
    {
        public AnimationTrigger Trigger;
      
    }


}