using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace SpawnerSystem
{

    public struct PlayerSpawnTag : IComponentData{ }

    public struct ItemSpawnTag : IComponentData
    {
        public int spawnrange { get { return 5; } }
    }
    public struct SpawnTag : IComponentData { }

}