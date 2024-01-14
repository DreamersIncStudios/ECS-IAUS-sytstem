using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Stats.Entities;

namespace DreamersInc.DamageSystem
{
    public partial class DeathSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();

            foreach (var (test, entity) in SystemAPI.Query<BaseCharacterComponent>().WithEntityAccess())
            {
                if (test.HealthRatio <= 0)
                {
                    var go = test.GOrepresentative;
                    Object.Destroy(go);
                    ecbSystem.CreateCommandBuffer().DestroyEntity(entity);

                }

            }
        }
    }
}