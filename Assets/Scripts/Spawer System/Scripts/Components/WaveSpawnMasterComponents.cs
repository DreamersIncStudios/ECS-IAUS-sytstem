using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;



namespace SpawnerSystem.WaveSystem
{
    [System.Serializable]
    public struct Wave
    {
        public int WaveNumber;
        public List<EnemyWaveSpec> EnemiesForWave;

        public uint MaxEnemyAtOnce;
        public int RewardGold;
        public int RewardEXP;
        public int RewardSpawnID;
        
    }
    [System.Serializable]
    public struct EnemyWaveSpec {
        public int spawnID;
        public int SpawnCount;
        public int level;
    }
    public struct WaveBufferComponent : IBufferElementData {
        public EnemyWaveSpec EnemySpecForWave;
        public int dispatchedCount;

        public static implicit operator EnemyWaveSpec(WaveBufferComponent w) { return w.EnemySpecForWave; }
        public static implicit operator WaveBufferComponent(EnemyWaveSpec e) { return new WaveBufferComponent() { EnemySpecForWave = e }; }

    }
    public struct WaveComponent : IComponentData {
        public int Level;
        public uint MaxEnemyAtOnce;
        public int RewardGold;
        public int RewardEXP;
        public int RewardSpawnID;
        public bool AllEnemiesDispatched;
        public int EnemiesDispatched;
        public int EnemiesDefeated;
    }
}