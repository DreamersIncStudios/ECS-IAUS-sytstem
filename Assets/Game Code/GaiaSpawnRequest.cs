using System.Collections.Generic;
using DreamersInc.BestiarySystem;
using DreamersIncStudio.GAIACollective;
using DreamersIncStudio.Moonshoot;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class GaiaSpawnRequest: SystemBase
{
    protected override void OnUpdate()
    {
        var spawnsToProcess = new List<LocalSpawnRequest>();
        Entities.WithoutBurst().WithNone<MaintShop>().ForEach((ref GaiaSpawnBiome b, in LocalToWorld transform)=>{
            if(b.SpawnRequests.IsEmpty) return;
  
            for (var index = 0; index < b.SpawnRequests.Length; index++)
            {
                // ReSharper disable once Unity.BurstLoadingManagedType
                spawnsToProcess.Add(new LocalSpawnRequest()
                {
                    SpawnRequest = b.SpawnRequests[index],
                    Position = transform.Position
                });

                b.SpawnRequests.RemoveAt(index);
            }
        }).Run();
        
        Entities.WithoutBurst().WithAll<MaintShop>().ForEach((Entity entity,ref GaiaSpawnBiome b, in LocalToWorld transform)=>{
            if(b.SpawnRequests.IsEmpty) return;

            for (var index = 0; index < b.SpawnRequests.Length; index++)
            {
                // ReSharper disable once Unity.BurstLoadingManagedType
                spawnsToProcess.Add(new LocalSpawnRequest()
                {
                    ParentToLink = entity,
                    SpawnRequest = b.SpawnRequests[index],
                    Position = transform.Position
                });

                b.SpawnRequests.RemoveAt(index);
            }
        }).Run();

        Entities.WithChangeFilter<Child>().ForEach((DynamicBuffer<Child> children, ref MaintShop Shop) =>
        {
            Shop.NumberOfWorkers= (uint)children.Length;
        }).Schedule();

        
        foreach (var request in spawnsToProcess)
        {
            
            for (var i = 0; i < request.SpawnRequest.Qty; i++)
            {
              BestiaryDB.SpawnNPC(request.SpawnRequest.SpawnID, request.Position,
                  request.SpawnRequest.HomeBiomeID,
                  request.SpawnRequest.PlayerLevel,
                  request.ParentToLink);
            }
        }
    }

    public struct LocalSpawnRequest
    {
        public Entity ParentToLink;
        public float3 Position;
        public SpawnRequest SpawnRequest;
    }
}



