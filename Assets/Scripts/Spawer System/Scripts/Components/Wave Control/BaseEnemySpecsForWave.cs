using Unity.Entities;

namespace SpawnerSystem.WaveSystem
{
    [GenerateAuthoringComponent]
    public struct BaseEnemySpecsForWave : IComponentData
    {
        public int EnemyId;
        public int BaseLevel;
        public int BaseRewardGold;
        public int BaseRewardEXP;
    }
}