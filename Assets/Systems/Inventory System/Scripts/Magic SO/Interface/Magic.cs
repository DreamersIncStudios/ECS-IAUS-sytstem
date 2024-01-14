 using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Dreamers.InventorySystem.AbilitySystem.Interfaces { 

    public interface IAbility
    {
        public AbilityAnimationInfo AnimInfo { get; }
        public void Activate(Entity CasterEntity);
        public void DisplayInfo(Entity Character);
        public void EquipAbility(Entity CasterEntity);
    }
    public interface IRecoveryAbility: IAbility {
        public uint Amount { get; }
        public uint ManaCost { get; }
        public GameObject VFX { get;}
    }
    public interface IAttackAbility:IAbility { 
        public uint DamageAmount { get; }
        public uint ManaCost { get; }
        public Vector3 Offset { get; }
        public GameObject VFX { get; }
    }

}