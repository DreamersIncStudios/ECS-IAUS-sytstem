using System;
using System.Collections.Generic;
using DreamersIncStudio.GAIACollective.Streaming.SceneManagement.SectionMetadata;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Biome = DreamersIncStudio.GAIACollective.GaiaSpawnBiome;
using Random = UnityEngine.Random;

namespace DreamersIncStudio.GAIACollective.Authoring
{

    public class GaiaSpawnBiome : MonoBehaviour, ISpawnBiome
    {    
        public uint BiomeID=> biomeID;
        [SerializeField] private uint biomeID;
        public int2 LevelRange=> levelRange;
        [SerializeField] private int2 levelRange;

        public List<SpawnData> SpawnData => spawnData;
        [SerializeField] private List<SpawnData> spawnData;

        public List<PackInfo> PacksToSpawn => packsToSpawn;
        [SerializeField] private List<PackInfo> packsToSpawn;
        public class Baker : Baker<GaiaSpawnBiome>
        {
            public override void Bake(GaiaSpawnBiome authoring)
            {
                var entity = GetEntity(TransformUsageFlags.WorldSpace);   
                AddComponent(entity, new Biome(authoring));
                var Radius = authoring.gameObject.layer switch
                {
                    6 => 750,
                    9 or 10 or 11 => 500,
                    26 => 250,
                    27 => 100,
                    28 => 85,
                    _ => 2250
                };
                AddComponent(entity, new GaiaOperationArea(authoring.transform.position, Radius));
            }
        }

  
    }
    
    public interface ISpawnBiome
    {
        public uint BiomeID { get; }
        public int2 LevelRange{ get; }
        public List<SpawnData> SpawnData{ get; }
        public List<PackInfo> PacksToSpawn{ get; }
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
        public FixedList512Bytes<SpawnRequest> SpawnRequests;
        public GaiaSpawnBiome( Authoring.ISpawnBiome gaiaSpawnBiome)
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
            SpawnRequests = new FixedList512Bytes<SpawnRequest>();
            Manager= Entity.Null;
            
        }

        public Entity Manager { get; set; }
    }
  

    [System.Serializable]
    public struct SpawnData
    {
        public SpawnScenario SpawnScenario;


        public uint SpawnID; // Spawn ID 4 digit number ABCC A is the Race, B is the Role, CC is the ID number.
        public TimesOfDay ActiveHours;
        [Range(1,25)]public uint Qty;
        private uint qtySpawned;
        public bool IsSatisfied => qtySpawned >= Qty;
            public bool Respawn => respawnTime <= 0.0f;
            private float respawnTime;
        [Range(0,30)]
        public int RespawnInterval;
  
            public void Spawn(ref FixedList512Bytes<SpawnRequest> spawnRequests,uint HomeBiomeID, int2 levelRange, uint playerLevel)
            {
                var cnt = Qty - qtySpawned;
                spawnRequests.Add(new SpawnRequest(SpawnID, HomeBiomeID, levelRange, playerLevel, cnt, ActiveHours));;
                ResetRespawn();
            }

            public void IncrementSpawned()
            {
                qtySpawned++;
            }
            public void IncrementSpawned(uint qty)
            {
                qtySpawned+= qty;
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
        public Size Size;
        public bool Created{get; set; }
        public int Qty { get; set; }

        public int QtyLimit => Size switch
                {
                    Size.small => 1,
                    Size.medium => 2,
                    Size.large => 3,
                    Size.huge => 6,
                    _ => throw new ArgumentOutOfRangeException()
                };
            
        
        public bool Satisfied => Qty >= QtyLimit;
    }

    public enum Size
    {
        small, medium, large, huge
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
    public struct SpawnRequest
    {
        public uint SpawnID;
        public uint HomeBiomeID;
        public int2 LevelRange;
        public uint PlayerLevel;
        public uint Qty;
        public TimesOfDay ActiveHours;
        public SpawnRequest(uint spawnID, uint homeBiomeID, int2 levelRange, uint playerLevel, uint cnt,
            TimesOfDay activeHours)
        {
            SpawnID = spawnID;
            HomeBiomeID = homeBiomeID;
            LevelRange = levelRange;
            PlayerLevel = playerLevel;
            Qty = cnt;
            ActiveHours = activeHours;
        }
    }
}