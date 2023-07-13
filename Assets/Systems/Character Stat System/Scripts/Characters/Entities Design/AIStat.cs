using System.Collections;
using System.Collections.Generic;
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

    }

    public partial class AIStatLinkSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().WithChangeFilter<BaseCharacterComponent>().ForEach((BaseCharacterComponent Base, ref AIStat aiStat )=> {

                aiStat.CurHealth = Base.CurHealth;
                aiStat.MaxHealth = Base.MaxHealth;
                aiStat.CurMana = Base.CurMana;
                aiStat.MaxMana= Base.MaxMana;
            }).Run();
        }
    }
}