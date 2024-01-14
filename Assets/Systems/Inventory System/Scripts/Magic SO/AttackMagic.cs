using Dreamers.InventorySystem.AbilitySystem.Interfaces;
using Stats;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
namespace Dreamers.InventorySystem.AbilitySystem
{
    public class AttackMagic : AbilitySO, IAttackAbility
    {
        public uint DamageAmount { get { return damageAmount; } }
        [SerializeField] uint damageAmount;
        public uint ManaCost { get => manaCost; }
        [SerializeField] uint manaCost;
        public GameObject VFX { get => vFX; }
        [SerializeField] GameObject vFX;
        public Vector3 Offset { get { return offset; } }

        [SerializeField] Vector2 offset;

        [SerializeField] Vector3 Size;


        public override void EquipAbility(Entity CasterEntity)
        {
            base.EquipAbility(CasterEntity);
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            BaseCharacterComponent stat = em.GetComponentData<BaseCharacterComponent>(CasterEntity);
            damageAmount = (uint)stat.GetStat((int)StatName.Magic_Offence).AdjustBaseValue * 10;
        }
        public override void Activate(Entity CasterEntity)
        {
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var statData = em.GetComponentData<BaseCharacterComponent>(CasterEntity);
            var transform = em.GetComponentData<LocalTransform>(CasterEntity);
            if (statData.CurMana >= ManaCost)
            {
                Debug.Log($"Casting  {Name} for {damageAmount}. It cost {ManaCost} mana to cast");
                statData.AdjustMana(-(int)ManaCost);
                if (VFX)
                {
                    var vfxGO = Instantiate(VFX, transform.Position + transform.Forward() * Offset.x + transform.Up() * Offset.y,
                        transform.Rotation);
                    if (vfxGO.GetComponentInChildren<ParticleDamage>())
                        vfxGO.GetComponentInChildren<ParticleDamage>().SetDamage(500);
                }


            }
        }

        public void DisplayInfo(Entity Character)
        {
            throw new System.NotImplementedException();
        }
    }
}