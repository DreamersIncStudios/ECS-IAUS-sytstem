using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DreamersIncStudio.GAIACollective
{
    [CreateAssetMenu(fileName = "Spawn Special", menuName = "GAIA/Spawn Special", order = 1)]
    public class GAIASpawnSO: ScriptableObject, ISpawnSpecial
    {
        public uint BiomeID => biomeID;
        [SerializeField]private uint biomeID;
        public List<SpawnData> SpawnData => spawnData;
        [SerializeField] private List<SpawnData> spawnData;
        public List<PackInfo> PacksToSpawn => packsToSpawn;
        [SerializeField] private List<PackInfo> packsToSpawn;
        
        
        public void LoadSpawnData()
        {
           
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var query = em.CreateEntityQuery(typeof(GaiaSpawnBiome));
            var biomeEntities = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in biomeEntities)
            {
                var biome = em.GetComponentData<GaiaSpawnBiome>(entity);
                if (biome.BiomeID != BiomeID) continue;
                foreach (var spawn in SpawnData)
                    biome.SpawnData.Add(spawn);
                foreach (var pack in PacksToSpawn)
                    biome.PacksToSpawn.Add(pack);
                em.SetComponentData(entity, biome);
            }
            
            biomeEntities.Dispose();
        }
    }
}