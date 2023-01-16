using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Stats;
using Unity.Entities;
using Unity.Burst.Intrinsics;

namespace DreamersInc.DamageSystem.Interfaces
{
    public interface IDamageable
    {
        bool Dead { get; }
        Entity SelfEntityRef { get; }
        void TakeDamage(int Amount, TypeOfDamage typeOf, Element element);
        void ReactToHit(float impact, Vector3 Test, Vector3 forward , TypeOfDamage typeOf = TypeOfDamage.Melee , Element element = Element.None);
    }


    public enum TypeOfDamage {Melee, MagicAoE, Projectile, Magic, Recovery}
    public enum Element { None, Fire, Water, Earth, Wind, Ice, Holy, Dark}
    public struct AdjustHealth : IComponentData {
        public int Value;
    }
    public struct AdjustMana : IComponentData
    {
        public int Value;   
    }

    public struct EntityHasDiedTag: IComponentData { public int Value; }

    public struct Player : IComponentData { }
    public struct Enemy : IComponentData { }
    public struct NPC : IComponentData { }


}