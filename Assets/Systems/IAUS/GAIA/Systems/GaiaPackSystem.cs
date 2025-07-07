using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Internal;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace DreamersIncStudio.GAIACollective
{
    [UpdateInGroup(typeof(GaiaUpdateGroup))]
    [UpdateAfter(typeof(GaiaSpawnSystem))]
    public partial struct GaiaPackSystem : ISystem
    {
        private EntityQuery packQuery;
        private EntityQuery agentsQuery;

        public void OnCreate(ref SystemState state)
        {
    
            packQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                    { ComponentType.ReadOnly(typeof(LocalTransform)), ComponentType.ReadWrite(typeof(Pack)) }
            });


        }

        public void OnUpdate(ref SystemState state)
        {
            var control = GetSingleton<GaiaControl>();
            var ecb = GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var depends = state.Dependency;
            depends = new FindLeader()
            {
                PackEntities = packQuery.ToEntityArray(Allocator.TempJob),
                PackLookup = state.GetComponentLookup<Pack>(false),
                ecb = ecb.CreateCommandBuffer(state.WorldUnmanaged)

            }.Schedule(depends);
            depends = new PackJoinJob()
            {
                PackEntities = packQuery.ToEntityArray(Allocator.TempJob),
                PackLookup = state.GetComponentLookup<Pack>(false),
                ecb = ecb.CreateCommandBuffer(state.WorldUnmanaged)
                
            }.Schedule(depends);
            state.Dependency= depends;

           
        }

    }

    [WithNone(typeof(PackMember))]

    public partial struct FindLeader : IJobEntity
    {
        public EntityCommandBuffer ecb;
        public NativeArray<Entity> PackEntities;
        public ComponentLookup<Pack> PackLookup;

        private void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, PassportAspect aspect)
        {
            foreach (var packEntity in PackEntities)
            {
                var pack = PackLookup[packEntity];
                if (pack.LeaderEntity != Entity.Null) continue;
                if (pack.Role != aspect.Role) continue;
                pack.LeaderEntity = entity;
                pack.MemberCount++;
                ecb.AddComponent(entity, new PackMember(packEntity));
                PackLookup[packEntity] = pack;
            }
        }
    }

    [WithNone(typeof(PackMember))]
    public partial struct PackJoinJob : IJobEntity
    {
        public EntityCommandBuffer ecb;
        public NativeArray<Entity> PackEntities;
        public ComponentLookup<Pack> PackLookup;

        private void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, PassportAspect aspect)
        {
            foreach (var packEntity in PackEntities)
            {
                var pack = PackLookup[packEntity];
                if (pack.Filled) continue;

                for (var i = 0; i < pack.Requirements.Length; i++)
                {
                    var requiredRole = pack.Requirements[i];
                    if (TryAssignRole(entity, aspect, ref requiredRole, packEntity, ref pack))
                    {
                        pack.Requirements[i] = requiredRole; // Update the modified role
                    }
                }

                PackLookup[packEntity] = pack; // Save the updated pack
            }
        }

        /// <summary>
        /// Attempts to assign the given entity to the provided role in the pack.
        /// </summary>
        /// <returns>True if the role was successfully assigned, false otherwise.</returns>
        private bool TryAssignRole(
            Entity entity,
            PassportAspect aspect,
            ref PackRole role,
            Entity packEntity,
            ref Pack pack)
        {
            if (aspect.Role != role.Role || role.QtyInfo.x <= role.QtyInfo.y) return false; // Role assignment failed
            pack.MemberCount++;
            role.QtyInfo.y++;
            ecb.AddComponent(entity, new PackMember(packEntity));
            return true; // Role assignment successful

        }
    }

}
