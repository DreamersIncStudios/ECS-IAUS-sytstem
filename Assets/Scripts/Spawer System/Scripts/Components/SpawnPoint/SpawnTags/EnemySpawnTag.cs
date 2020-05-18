using Unity.Entities;

namespace SpawnerSystem
{
    [GenerateAuthoringComponent]
    public struct EnemySpawnTag : IComponentData
    {
        public int MaxLevel;
    }

}