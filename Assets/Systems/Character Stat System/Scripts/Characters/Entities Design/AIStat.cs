using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Entities;
using UnityEngine;

namespace Stats.Entities
{
    public struct AIStat : IComponentData
    {
        public float CurHealth, MaxHealth, CurMana, MaxMana;
        public float HealthRatio => CurHealth/ MaxHealth;
        public float ManaRatio => CurMana/ MaxMana;

        public float Speed;

        public AIStat(int speed)
        {
            //Todo Replace with value from Character Stat 
            this.Speed = speed;
            CurHealth = 0;
            MaxHealth = 0;
            CurMana = 0;
            MaxMana = 0;
        }

    }

    public partial class AIStatLinkSystem : SystemBase
    {
        [SuppressMessage("ReSharper", "Unity.BurstLoadingManagedType")]
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().WithChangeFilter<BaseCharacterComponent>().ForEach((BaseCharacterComponent baseStat, ref AIStat aiStat )=> {

                aiStat.CurHealth = baseStat.CurHealth;
                aiStat.MaxHealth = baseStat.MaxHealth;
                aiStat.CurMana = baseStat.CurMana;
                aiStat.MaxMana= baseStat.MaxMana;
            }).Run();
        }
    }
}