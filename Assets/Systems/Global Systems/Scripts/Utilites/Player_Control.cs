using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

namespace DreamersInc
{
    public struct Player_Control : IComponentData
    {

        public bool InSafeZone;
    }
    public struct NPC_Control : IComponentData
    {

        public bool InSafeZone;
    }
    public partial class ChargeSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((ref ControllerInfo PC) =>
            {
                if (PC.ChargedHeavyAttackb || PC.ChargedLightAttackb || PC.ChargedProjectileb)
                    PC.ChargedTime += SystemAPI.Time.DeltaTime;
            }).Run();
        }
    }
}