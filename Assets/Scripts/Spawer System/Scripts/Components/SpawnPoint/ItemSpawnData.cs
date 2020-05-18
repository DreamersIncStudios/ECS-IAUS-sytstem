using Unity.Entities;
using UnityEngine;

namespace SpawnerSystem
{
    [GenerateAuthoringComponent]
    [System.Serializable]
    public struct ItemSpawnData : IBufferElementData
    {
        [System.Serializable]
        public struct Data
        {
            public int SpawnID;
            public bool Spawn;
            public int SpawnCount;

            public float probabilityWeight;
            [HideInInspector]
            public float probabilityPercent;
            [HideInInspector]
            public float probabilityRangeFrom;
            [HideInInspector]
            public float probabilityRangeTo;
        }
        public Data spawnData;

        public static implicit operator Data(ItemSpawnData e) { return e; }

        public static implicit operator ItemSpawnData(Data e) { return new ItemSpawnData { spawnData = e }; }
    }
}