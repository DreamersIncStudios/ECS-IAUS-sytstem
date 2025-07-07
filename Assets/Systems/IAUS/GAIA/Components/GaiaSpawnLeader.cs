using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DreamersIncStudio.GAIACollective
{
    /// <summary>
    /// Entity is a spwawn point and leader of a pack. Spawn point can't move.
    /// 
    /// </summary>
    public struct GaiaSpawnLeader : IComponentData
    {
        public FixedList512Bytes<SpawnData> SpawnData;
        public FixedList128Bytes<PackRole>  Requirements;
        public float CohesionFactor;
        public float SeparationFactor;
        public float AlignmentFactor;
        public float3 HerdCenter; // Central point for the herd
        public int MemberCount;
        public uint BiomeID;
        public Role Role;
 
        public bool Filled
        {
            get
            {
                if (Requirements.Length == 0)
                {
                    Debug.Log("no requirements");
                    return true;
                }

                // Manual loop to get the sum of QtyInfo.x
                var count = 0;
                for (var i = 0; i < Requirements.Length; i++)
                {
                    count += Requirements[i].QtyInfo.x;
                }

                return count == MemberCount;
            }
        }
        
    }
}