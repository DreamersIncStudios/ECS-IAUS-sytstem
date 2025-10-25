using System.Collections.Generic;
using DreamersIncStudio.GAIACollective;
using DreamersIncStudio.GAIACollective.Authoring;
using DreamersIncStudio.GAIACollective.Streaming.SceneManagement.SectionMetadata;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Biome = DreamersIncStudio.GAIACollective.GaiaSpawnBiome;
namespace DreamersIncStudio.Moonshoot
{
    public class MaintShopAuthoring : MonoBehaviour, ISpawnBiome
    {

        public uint BiomeID=> biomeID;
        [SerializeField] private uint biomeID;
        public int2 LevelRange=> levelRange;
        [SerializeField] private int2 levelRange;

        public List<SpawnData> SpawnData => spawnData;
        [SerializeField] private List<SpawnData> spawnData;

        public List<PackInfo> PacksToSpawn => packsToSpawn;
        [SerializeField] private List<PackInfo> packsToSpawn;
        public SpawnScenario SpawnScenario => spawnScenario;
        [SerializeField] private SpawnScenario spawnScenario;
        
        [Range(50, 400)] [SerializeField] private float range = 150;
        [Range(1, 20)] [SerializeField] private uint workerCount = 1;

        private class MaintShopBaker : Baker<MaintShopAuthoring>
        {
            public override void Bake(MaintShopAuthoring authoring)
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
                AddComponent(entity, new MaintShop()
                {
                    Range = authoring.range,
                });

            }
        }


    }
}
