using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace SpawnerSystem.WaveSystem
{
    [GenerateAuthoringComponent]
    public struct WaveBuffer : IBufferElementData
    {
        [System.Serializable]
        public struct Data
        {
            public int Level;
            public int SpawnCount;
            public int MaxSpawnsPerSpawnRoutine; // This many entities will spawn when a spawn call happens Someone rename this :P
            public int Modlevel;
            public int SpawnLevel(int Base) { return Base + Modlevel; }
            public int dispatchedCount;
            public bool AllEnemiesDispatched { get { return SpawnCount <= dispatchedCount; } }
        }

        public Data spawnData;
        public static implicit operator Data(WaveBuffer e) { return e; }
        public static implicit operator WaveBuffer(Data e) { return new WaveBuffer { spawnData = e }; }
    }


}