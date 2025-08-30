using Unity.Entities;
using Unity.Mathematics;

namespace DreamersIncStudio.GAIACollective
{
    public struct GaiaLevelManager:IComponentData
    {
        public int ID;
        public int2 Bounds;
        public SpawnScenario SpawnScenario;

    }

    public enum SpawnScenario
    {
        DoNotSpawn,
        NormalSpawn,
        BossSpawn,
        SpecialSpawn
    }
}