using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Stats.Entities
{
    public struct AIStat : IComponentData
    {
        public float CurHealth, MaxHealth, CurMana, MaxMana;
        public float HealthRatio => CurHealth/ MaxHealth;
        public float ManaRatio => CurMana/ MaxMana;

        public int Level;
        public float Speed;

        public AIStat(int speed)
        {
            //Todo Replace with value from Character Stat 
            this.Speed = speed;
            CurHealth = 0;
            MaxHealth = 0;
            CurMana = 0;
            MaxMana = 0;
            Level = 1;
        }

    }


    public partial class AIStatLinkSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((ref AIStat aiStat, in Parent parent) =>
            {
                var baseStat = EntityManager.GetComponentData<BaseCharacterComponent>(parent.Value);

                aiStat.CurHealth = baseStat.CurHealth;
                aiStat.MaxHealth = baseStat.MaxHealth;
                aiStat.CurMana = baseStat.CurMana;
                aiStat.MaxMana = baseStat.MaxMana;
                aiStat.Level = baseStat.Level;
            }).Run();
        }
    }
}