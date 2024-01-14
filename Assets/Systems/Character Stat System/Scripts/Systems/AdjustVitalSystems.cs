using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Stats;
using Unity.Entities;
using Unity.Burst;
using DreamersInc.DamageSystem.Interfaces;
using Unity.Burst.Intrinsics;
using Unity.Assertions;
using Stats.Entities;

namespace DreamersInc.DamageSystem
{
    public partial class AdjustVitalSystems : SystemBase
    {

        protected override void OnUpdate()
        {

            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ECB = ecbSingleton.CreateCommandBuffer(World.Unmanaged);

            Entities.WithoutBurst().ForEach((Entity entity,BaseCharacterComponent character, in AdjustHealth mod) => {
                character.AdjustHealth(mod.Value);
                if (character.CurHealth <= 0)
                {
                    ECB.AddComponent<EntityHasDiedTag>(entity);
                }
                ECB.RemoveComponent<AdjustHealth>(entity);

            }).Run();


            Entities.WithoutBurst().ForEach((Entity entity,BaseCharacterComponent mana, in AdjustMana mod) => {
                mana.AdjustMana(mod.Value);

                ECB.RemoveComponent<AdjustMana>(entity);
            }).Run();

        }
    }
}