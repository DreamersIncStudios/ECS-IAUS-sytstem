using Dreamers.InventorySystem.AbilitySystem.Interfaces;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Dreamers.InventorySystem.AbilitySystem
{
    public class RecoveryMagic : AbilitySO, IRecoveryAbility
    {
        public uint Amount { get { return amount; } }
        [SerializeField] uint amount;
        public uint ManaCost { get => manaCost; }
        [SerializeField] uint manaCost;
        public GameObject VFX { get => vFX;}
        [SerializeField] GameObject vFX;


        public override void Activate(Entity CasterEntity)
        {
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var statData = em.GetComponentData<BaseCharacterComponent>(CasterEntity);
            Vector3 positon = em.GetComponentData<LocalTransform>(CasterEntity).Position;
            if (statData.CurMana >= ManaCost)
            {
                statData.AdjustMana(-(int)ManaCost);
                statData.AdjustHealth((int)Amount);
                if (VFX)
                    Instantiate(VFX, positon, VFX.transform.rotation);
            }
        }

        public void DisplayInfo(Entity Character)
        {
            throw new System.NotImplementedException();
        }
    }
}