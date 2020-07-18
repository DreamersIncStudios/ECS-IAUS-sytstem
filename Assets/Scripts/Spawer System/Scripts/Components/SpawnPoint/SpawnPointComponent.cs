using Unity.Entities;

namespace SpawnerSystem{
    [GenerateAuthoringComponent]
    public struct SpawnPointComponent : IComponentData
    {
        public bool Temporoary;
        public uint SpawnPointID; // Using Entity index number. Spawn ID not being used in system consider 
        public bool IDassigned;
    }
}
