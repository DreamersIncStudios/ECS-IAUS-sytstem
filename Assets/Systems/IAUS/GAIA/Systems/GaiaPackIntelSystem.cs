using AISenses;
using Unity.Collections;
using Unity.Entities;
using Resources = AISenses.Resources;

namespace DreamersIncStudio.GAIACollective
{
    [UpdateInGroup(typeof(GaiaUpdateGroup))]
    [UpdateAfter(typeof(GaiaPackManagementSystem))]
    public partial class GaiaPackIntelSystem : SystemBase
    {

        private BufferLookup<Enemies> enemiesBufferLookup;
        private BufferLookup<Allies> allyBufferLookup;
        private BufferLookup<Resources> resourceBufferLookup;
        private BufferLookup<PlacesOfInterest> placeBufferLookup;
        protected override void OnCreate()
        {
            enemiesBufferLookup = SystemAPI.GetBufferLookup<Enemies>(true);
            allyBufferLookup = SystemAPI.GetBufferLookup<Allies>(true);
            resourceBufferLookup = SystemAPI.GetBufferLookup<Resources>(true);
            placeBufferLookup = SystemAPI.GetBufferLookup<PlacesOfInterest>(true);
        }

   protected override void OnUpdate()
        {
            enemiesBufferLookup.Update(this);
            allyBufferLookup.Update(this);
            resourceBufferLookup.Update(this);
            placeBufferLookup.Update(this);
            var enemiesLookupRO = enemiesBufferLookup;
            var allyLookupRO = allyBufferLookup;
            var resourceLookupRO = resourceBufferLookup;
            var placeLookupRO = placeBufferLookup;

            Entities
                .WithName("GaiaPackIntel_AggregateEnemies")
                .WithReadOnly(enemiesLookupRO)
                .ForEach((Entity entity, DynamicBuffer<PackList> packLists, DynamicBuffer<Enemies> enemies) =>
                {
                    enemies.Clear();
                    // Aggregate enemies from all pack members
                    for (int i = 0; i < packLists.Length; i++)
                    {
                        var member = packLists[i];
                        if (member.PackMember == entity) continue; // avoid aliasing with self

                        if (!enemiesLookupRO.HasBuffer(member.PackMember))
                            continue;

                        var memberEnemies = enemiesLookupRO[member.PackMember];
                        if (memberEnemies.IsEmpty)
                            continue;

                        for (int j = 0; j < memberEnemies.Length; j++)
                        {
                            enemies.Add(memberEnemies[j]);
                        }
                    }
                })
                .Run();
            
            Entities
                .WithName("GaiaPackIntel_AggregateAlly")
                .WithReadOnly(allyLookupRO)
                .ForEach((Entity entity, DynamicBuffer<PackList> packLists, DynamicBuffer<Allies> allies) =>
                {
                    allies.Clear();
                    // Aggregate enemies from all pack members
                    for (int i = 0; i < packLists.Length; i++)
                    {
                        var member = packLists[i];
                        if (member.PackMember == entity) continue; // avoid aliasing with self

                        if (!allyLookupRO.HasBuffer(member.PackMember))
                            continue;

                        var memberEnemies = allyLookupRO[member.PackMember];
                        if (memberEnemies.IsEmpty)
                            continue;

                        for (int j = 0; j < memberEnemies.Length; j++)
                        {
                            allies.Add(memberEnemies[j]);
                        }
                    }
                })
                .Run();  
            
            Entities
                .WithName("GaiaPackIntel_AggregateResource")
                .WithReadOnly(resourceLookupRO)
                .ForEach((Entity entity, DynamicBuffer<PackList> packLists, DynamicBuffer<Resources> resources) =>
                {
                    resources.Clear();
                    // Aggregate enemies from all pack members
                    for (int i = 0; i < packLists.Length; i++)
                    {
                        var member = packLists[i];
                        if (member.PackMember == entity) continue; // avoid aliasing with self

                        if (!resourceLookupRO.HasBuffer(member.PackMember))
                            continue;

                        var memberEnemies = resourceLookupRO[member.PackMember];
                        if (memberEnemies.IsEmpty)
                            continue;

                        for (int j = 0; j < memberEnemies.Length; j++)
                        {
                            resources.Add(memberEnemies[j]);
                        }
                    }
                })
                .Run();     
            
            Entities
                .WithName("GaiaPackIntel_AggregatePlaces")
                .WithReadOnly(placeLookupRO)
                .ForEach((Entity entity, DynamicBuffer<PackList> packLists, DynamicBuffer<PlacesOfInterest> places) =>
                {
                    places.Clear();
                    // Aggregate enemies from all pack members
                    for (int i = 0; i < packLists.Length; i++)
                    {
                        var member = packLists[i];
                        if (member.PackMember == entity) continue; // avoid aliasing with self

                        if (!placeLookupRO.HasBuffer(member.PackMember))
                            continue;

                        var memberEnemies = placeLookupRO[member.PackMember];
                        if (memberEnemies.IsEmpty)
                            continue;

                        for (int j = 0; j < memberEnemies.Length; j++)
                        {
                            places.Add(memberEnemies[j]);
                        }
                    }
                })
                .Run();

        }
        
        private partial struct PackIntelUpdateJob : IJobEntity
        {
            [ReadOnly]public BufferLookup<Enemies> EnemiesBufferLookup;

            [ReadOnly]public BufferLookup<Allies> AllyBufferLookup;
            [ReadOnly]public BufferLookup<Resources> ResourceBufferLookup;
            [ReadOnly]public BufferLookup<PlacesOfInterest> PlaceBufferLookup;
            private void Execute(Entity entity, in Pack pack, DynamicBuffer<PackList> packLists,DynamicBuffer<Enemies> enemies
            )   
            {
           

              
            }
        }
    }
}