using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Dreamers.InventorySystem.AbilitySystem;

namespace DreamersInc.ComboSystem
{

    [System.Serializable]
    public class Command : IComponentData
    {
        public Queue<AnimationTrigger> InputQueue;
        public Queue<string> MagicInputQueue;
        public bool HasMagicSpell => MagicInputQueue.Count > 0;
        public bool QueueIsEmpty => InputQueue.Count == 0;
        public bool WeaponIsEquipped;
        public AnimatorStateInfo StateInfo { get; set; }
        public float currentStateExitTime;
        public bool BareHands;
        public bool AlwaysDrawnWeapon { get; set; } // Todo set this true or false in the equip method
        public bool TakeInput => (WeaponIsEquipped || BareHands) && !QueueIsEmpty && StateInfo.normalizedTime > currentStateExitTime;
        public bool TransitionToLocomotion => !StateInfo.IsTag("Locomotion") && StateInfo.normalizedTime > .95f;
        public AbilityList EquippedAbilities;
        public float InputTimer;
        public float InputTimeReset;
      [SerializeField]  public bool CanInputAbilities => InputTimer >= 0.0f;
    }
}