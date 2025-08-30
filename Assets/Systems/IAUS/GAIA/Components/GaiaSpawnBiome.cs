using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Biome = DreamersIncStudio.GAIACollective.GaiaSpawnBiome;
using Random = UnityEngine.Random;

namespace DreamersIncStudio.GAIACollective.Authoring
{
    public class GaiaSpawnBiome : MonoBehaviour
    {    
        public uint BiomeID;
        public int2 LevelRange;
        public List<SpawnData> SpawnData;
        public List<PackInfo> PacksToSpawn;
        public class Baker : Baker<GaiaSpawnBiome>
        {
            public override void Bake(GaiaSpawnBiome authoring)
            {
                var entity = GetEntity(TransformUsageFlags.WorldSpace);   
                AddComponent(entity, new Biome(authoring));
            }
        }

  
    }
}
namespace DreamersIncStudio.GAIACollective
{
    public struct GaiaSpawnBiome: IComponentData
    {
        public uint BiomeID;
        public int2 LevelRange;
        public FixedList512Bytes<SpawnData> SpawnData;
        public FixedList128Bytes<PackInfo> PacksToSpawn;
        public GaiaSpawnBiome( Authoring.GaiaSpawnBiome gaiaSpawnBiome)
        {
            BiomeID = gaiaSpawnBiome.BiomeID;
            LevelRange = gaiaSpawnBiome.LevelRange;
            SpawnData = new FixedList512Bytes<SpawnData>();
            PacksToSpawn = new FixedList128Bytes<PackInfo>();
            foreach (var spawn in gaiaSpawnBiome.SpawnData)
            {
                SpawnData.Add(spawn);
            }

            foreach (var pack in gaiaSpawnBiome.PacksToSpawn )
            {
                PacksToSpawn.Add(pack);
            }
        }
    }
  

    [System.Serializable]
    public struct SpawnData
    {
        public uint SpawnID;
        public TimesOfDay ActiveHours;
        public uint Qty;
        private uint qtySpawned;
        public bool IsSatisfied => qtySpawned >= Qty;
            public bool Respawn => respawnTime <= 0.0f;
        private float respawnTime;
        [Range(0,20)]
        public int RespawnInterval;
            public void Spawn(uint HomeBiomeID, int2 levelRange, uint playerLevel)
            {
                var entities = new List<Entity>();
                var cnt = Qty - qtySpawned;
                //Todo Update implementation 
                ResetRespawn();
         
            }

            public void ResetRespawn()
            {
                var interval = 60.0f * RespawnInterval;
                respawnTime = Random.Range(.855f*interval, 1.075f* interval);
                
            }

            public void Countdown(float time)
            {
                respawnTime -= time;
            }

            public void SpawnKiller()
            {
                if(qtySpawned==0) return;
                qtySpawned--;
            }
    }

    [System.Serializable]
    public struct PackInfo
    {
        public PackType PackType;
        public int Qty { get; set; }
        public int QtyLimit;
        public bool Satisfied => Qty >= QtyLimit;
    }

    public enum PackType
    {
        Assault,
        Support,
        Transport,
        Scavengers,
        Recon,
        Combat,
        Acquisition
    }

}