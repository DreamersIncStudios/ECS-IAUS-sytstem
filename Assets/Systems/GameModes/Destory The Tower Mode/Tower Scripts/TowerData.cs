using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace GameModes.DestroyTheTower.TowerSystem
{
    public struct TowerData : IComponentData
    {
        public int level;
        public float RepairRate;
        public float EnergyRecoverRate;
        public float GatherResourcesRate;

        public float ResourcesGathered;
        public float EnergyLevel;
       
    }
}