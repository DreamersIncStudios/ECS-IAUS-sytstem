using System.Collections.Generic;

namespace DreamersIncStudio.GAIACollective
{
    public interface ISpawnSpecial
    {
        public uint BiomeID { get; }
        public List<SpawnData> SpawnData { get; }
        public List<PackInfo> PacksToSpawn { get; }
    
        
        
        public void LoadSpawnData();
    }
}