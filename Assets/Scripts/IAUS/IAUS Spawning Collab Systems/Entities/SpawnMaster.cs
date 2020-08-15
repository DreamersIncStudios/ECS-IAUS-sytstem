using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;


namespace SpawnerSystem.WaveSystem
{
    public class SpawnMaster : MonoBehaviour
    {
        public List<Wave> EnemyWaves;
        public int CurLevel;
        bool WaveCreated { get { return EnemyWaves.Count > 0; } }
        private void Start()
        { 
            if (!WaveCreated)
            {
                Debug.Log("Waves have not been created", this);
                return;
            }
            foreach (Wave wave in EnemyWaves)
            {
                var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
                var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                Entity entity = entityManager.CreateEntity();
                var setup = new WaveComponent()
                {
                    Level = EnemyWaves.IndexOf(wave),
                    RewardEXP = wave.RewardEXP,
                    RewardGold = wave.RewardGold,
                    RewardSpawnID = wave.RewardSpawnID,
                    MaxEnemyAtOnce = wave.MaxEnemyAtOnce
                    
                };
                entityManager.AddComponentData(entity, setup);

                DynamicBuffer<WaveBufferComponent> buffer = entityManager.AddBuffer<WaveBufferComponent>(entity);
                foreach (EnemyWaveSpec Enemy in wave.EnemiesForWave)
                {
                    buffer.Add(new WaveBufferComponent() { EnemySpecForWave = Enemy });
                }
             
            }

        }


    }

}