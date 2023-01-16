using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace DreamersInc.ComboSystem
{

    [System.Serializable]
    public class Command : IComponentData
    {
        public Queue<AnimationTrigger> InputQueue;
        public bool QueueIsEmpty => InputQueue.Count == 0;
        public bool WeaponIsEquipped;
        public AnimatorStateInfo StateInfo { get; set; }
        public float currentStateExitTime;
        public bool BareHands;
        public bool TakeInput => (WeaponIsEquipped || BareHands) && !QueueIsEmpty && StateInfo.normalizedTime > currentStateExitTime;
        public bool TransitionToLocomotion => !StateInfo.IsTag("Locomotion") && StateInfo.normalizedTime > .95f;
         
    }
}