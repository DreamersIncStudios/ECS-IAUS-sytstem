using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DreamersIncStudio.GAIACollective
{
    /// <summary>
    /// Entity is a spawn point and leader of a pack. 
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
        public GaiaSpawnLeader(List<PackRole> requirement, uint BiomeID, Role Roles, TimesOfDay activeHours )
        {
            SpawnData = new FixedList512Bytes<SpawnData>();
       

            Requirements = new FixedList128Bytes<PackRole>();
            foreach (var role in requirement)
            {
                Requirements.Add(role);
                SpawnData.Add(new SpawnData()
                {
                    ActiveHours = activeHours,
                    RespawnInterval = 10,
                    Qty = (uint)role.QtyInfo.x,
                    SpawnID = 0
                });
            }

            CohesionFactor = 1.0f;
            SeparationFactor = 2.0f;
            AlignmentFactor = 0.5f;
            this.BiomeID = BiomeID;
            Role = Roles;
            HerdCenter = default;
            MemberCount = 0;
           
        }
        public static GaiaSpawnLeader AssaultTeam(uint BiomeID, TimesOfDay activeHours) => new GaiaSpawnLeader()
        {
            Requirements =  new FixedList128Bytes<PackRole>()
            {
                new PackRole(Role.Recon, new int2(1,0)),
                new PackRole(Role.Combat, new int2(1,0)),
                new PackRole(Role.Scavengers, new int2(1,0)),
                new PackRole(Role.Transport, new int2(1,0)),
                new PackRole(Role.Acquisition, new int2(1,0)),
                new PackRole(Role.Support, new int2(1,0)),
            },
            SpawnData = new FixedList512Bytes<SpawnData>()
            {
                //Recon
                new SpawnData(){  ActiveHours = activeHours, 
                    RespawnInterval = 10,
                    Qty = (uint)1,
                    SpawnID = 0},
                //Combat
                new SpawnData(){  ActiveHours = activeHours, 
                    RespawnInterval = 10,
                    Qty = (uint)1,
                    SpawnID = 0},
                //Scavengers
                new SpawnData(){  ActiveHours = activeHours,
                    RespawnInterval = 10,
                    Qty = (uint)1,
                    SpawnID = 0},
                //Transport
                new SpawnData(){  ActiveHours = activeHours,
                    RespawnInterval = 10,
                    Qty = (uint)1,
                    SpawnID = 0},
                //Acquistion 
                new SpawnData(){  ActiveHours = activeHours,
                    RespawnInterval = 10,
                    Qty = (uint)1,
                    SpawnID = 0},
                //Support
                new SpawnData(){  ActiveHours = activeHours,
                    RespawnInterval = 10,
                    Qty = (uint)1,
                    SpawnID = 0},
            },
            CohesionFactor = 1.0f,
            SeparationFactor = 2.0f,
            AlignmentFactor = 0.5f,
            BiomeID = BiomeID,
            Role = Role.Combat,
            
        };
        public static GaiaSpawnLeader Support(uint BiomeID, TimesOfDay activeHours) => new GaiaSpawnLeader()
        {
            Requirements =  new FixedList128Bytes<PackRole>()
            {
                new PackRole(Role.Recon, new int2(3,0)),
                new PackRole(Role.Combat, new int2(2,0)),
                new PackRole(Role.Scavengers, new int2(0,0)),
                new PackRole(Role.Transport, new int2(1,0)),
                new PackRole(Role.Acquisition, new int2(0,0)),
                new PackRole(Role.Support, new int2(3,0)),
            },
            SpawnData = new FixedList512Bytes<SpawnData>()
            {
                //Recon
                new SpawnData(){  ActiveHours = activeHours, 
                    RespawnInterval = 10,
                    Qty = (uint)3,
                    SpawnID = 0},
                //Combat
                new SpawnData(){  ActiveHours = activeHours, 
                    RespawnInterval = 10,
                    Qty = (uint)2,
                    SpawnID = 0},
                //Scavengers
                //Transport
                new SpawnData(){  ActiveHours = activeHours,
                    RespawnInterval = 10,
                    Qty = (uint)1,
                    SpawnID = 0},
                //Acquistion
                ////Support
                new SpawnData(){  ActiveHours = activeHours,
                    RespawnInterval = 10,
                    Qty = (uint)3,
                    SpawnID = 0},
            },
            
            CohesionFactor = 1.0f,
            SeparationFactor = 2.0f,
            AlignmentFactor = 0.5f,
            BiomeID = BiomeID,
            Role = Role.Support
        };

    }
}