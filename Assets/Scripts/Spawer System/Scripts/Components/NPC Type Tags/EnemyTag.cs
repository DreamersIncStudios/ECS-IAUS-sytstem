using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace SpawnerSystem
{
    public struct EnemyTag : IComponentData {
        public bool Destory;
    }

}
