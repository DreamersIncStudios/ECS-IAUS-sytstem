using System;
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

        public FixedList128Bytes<PackRole> Requirements;
        public Entity LeaderEntity;
        public float CohesionFactor;
        public float SeparationFactor;
        public float AlignmentFactor;
        public float3 HerdCenter; // Central point for the herd
        public int MemberCount;
        public uint BiomeID;
        public Role Role;

        private static int2 Need(int required, Size size) => new int2(required * Mod(size), 0);

        private static int Mod(Size size)
        {
            return size switch
            {
                Size.small => 1,
                Size.medium => 2,
                Size.large => 3,
                Size.huge => 4,
                _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
            };
        }

        public static Pack AssaultTeam(uint BiomeID, Size size)=> new Pack()
        {
            Requirements = new FixedList128Bytes<PackRole>()
            {
                new PackRole(Role.Recon, Need(2, size)),
                new PackRole(Role.Combat, Need(5, size)),
                new PackRole(Role.Scavengers, Need(1, size)),
                new PackRole(Role.Transport, Need(0, size)),
                new PackRole(Role.Acquisition, Need(0, size)),
                new PackRole(Role.Support, Need(2, size))
            },
            CohesionFactor = 1.0f,
            SeparationFactor = 2.0f,
            AlignmentFactor = 0.5f,
            BiomeID = BiomeID,
            Role = Role.Combat
        };
    



        public static Pack Support(uint BiomeID,Size size) => new Pack()
        {
            Requirements =  new FixedList128Bytes<PackRole>()
            {
                new PackRole(Role.Recon, Need(3, size)),
                new PackRole(Role.Combat, Need(2, size)),
                new PackRole(Role.Scavengers, Need(0, size)),
                new PackRole(Role.Transport,Need(1, size)),
                new PackRole(Role.Acquisition, Need(0, size)),
                new PackRole(Role.Support, Need(3, size)),
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