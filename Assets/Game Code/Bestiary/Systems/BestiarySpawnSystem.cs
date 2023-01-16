using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace DreamersInc.BestiarySystem
{
    public partial class BestiarySpawnSystem : SystemBase
    {

        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            foreach (var (tag,entity) in SystemAPI.Query<SpawnTag>().WithEntityAccess()) {
                for (int i = 0; i < tag.Qty; i++)
                {
                   BestiaryDB.SpawnCreature(tag.ID);
                    ecb.DestroyEntity(entity);
                }
            }
        }
    }
}