using Unity.Entities;
using UnityEngine;
namespace SpawnerSystem {
    [System.Serializable]
    [GenerateAuthoringComponent]
    public struct EnemySpawnData : IBufferElementData
    { [System.Serializable]
       public struct Data
        {

            public int SpawnID;
            public bool Spawn { get { return SpawnCount > 0; } }
            public int SpawnCount;
        }
        public Data spawnData;
   

        public static implicit operator int(EnemySpawnData e) { return e; }

        public static implicit operator EnemySpawnData(Data e) { return new EnemySpawnData { spawnData = e }; }
    }



}