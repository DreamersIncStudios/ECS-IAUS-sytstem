using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace DreamersIncStudio.GAIACollective
{
    public struct Pack : IComponentData
    {
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
        public FixedList128Bytes<PackRole>  Requirements;
        public Entity LeaderEntity;
        public float CohesionFactor;
        public float SeparationFactor;
        public float AlignmentFactor;
        public float3 HerdCenter; // Central point for the herd
        public int MemberCount;
        public uint BiomeID;
        public Role Role;
        public static Pack AssaultTeam(uint BiomeID) => new Pack()
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
            CohesionFactor = 1.0f,
            SeparationFactor = 2.0f,
            AlignmentFactor = 0.5f,
            BiomeID = BiomeID,
            Role = Role.Combat
        };
        
        
        public static Pack Support(uint BiomeID) => new Pack()
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
            CohesionFactor = 1.0f,
            SeparationFactor = 2.0f,
            AlignmentFactor = 0.5f,
            BiomeID = BiomeID,
            Role = Role.Support
            
        };
    }

    public struct PackMember : IComponentData
    {
        public Entity PackEntity;

        public PackMember(Entity packEntity)
        {
          PackEntity = packEntity;
        }
    }

    public struct PackRole
    {
        public Role Role;
        /// <summary>
        /// Represents the quantity information for a role: 
        /// x = required amount, y = currently assigned amount.
        /// </summary>
        public int2 QtyInfo; 

        public PackRole(Role role, int2 qtyInfo)
        {
           Role = role;
           QtyInfo = qtyInfo;
        }
    }

    public enum Role
    {
        Recon,
        Combat, 
       Scavengers,
       Transport,
       Acquisition,
        Support 
        
    }
}