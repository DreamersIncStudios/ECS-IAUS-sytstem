using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace DreamersInc.ComboSystem
{
    [GenerateAuthoringComponent]
    public class Command : IComponentData
    {
        public Queue<AnimationTrigger> InputQueue;
        public bool QueueIsEmpty => InputQueue.Count == 0;
        public bool WeaponIsEquipped;
        public AnimatorStateInfo StateInfo;
        public float currentStateExitTime;
        public bool BareHands;
        public bool TakeInput => (WeaponIsEquipped || BareHands) && !QueueIsEmpty && StateInfo.normalizedTime > currentStateExitTime;
        public bool TransitionToLocomotion => !StateInfo.IsTag("Locomotion") && StateInfo.normalizedTime > .95f;
    }
}