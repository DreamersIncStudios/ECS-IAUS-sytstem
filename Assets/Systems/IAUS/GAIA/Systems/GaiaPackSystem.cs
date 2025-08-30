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
        private ComponentLookup<Pack> packLookup;
        private ComponentLookup<LocalToWorld> transformLookup;
        public void OnCreate(ref SystemState state)
        {
            packQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                    { ComponentType.ReadOnly(typeof(LocalTransform)), ComponentType.ReadWrite(typeof(Pack)) }
            });
            packLookup = state.GetComponentLookup<Pack>(false);
            transformLookup = state.GetComponentLookup<LocalToWorld>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            var control = GetSingleton<GaiaControl>();
            var ecb = GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var packs = packQuery.ToEntityArray(Allocator.TempJob);
            var depends = state.Dependency;
            packLookup.Update(ref state);
            transformLookup.Update(ref state);
            
            var cmd = ecb.CreateCommandBuffer(state.WorldUnmanaged);
            var leaders = new NativeParallelHashSet<Entity>(packs.Length, Allocator.TempJob);

            depends = new FindLeader()
            {
                PackEntities = packs,
                PackLookup = packLookup,
                ecb = cmd,
                LeadersAssigned = leaders.AsParallelWriter()

            }.Schedule(depends);

            depends = new PackJoinJob()
            {
                PackEntities = packs,
                PackLookup = packLookup,
                ecb = cmd,
                LeadersAssigned = leaders

            }.Schedule(depends);
            
            depends = new UpdatePackPositions()
            {
                FindTransform = transformLookup
            }.Schedule(depends);
            
            depends = packs.Dispose(depends);
            state.Dependency = depends;


        }


        [WithNone(typeof(PackMember))]

        partial struct FindLeader : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public NativeArray<Entity> PackEntities;
            public ComponentLookup<Pack> PackLookup;
            public NativeParallelHashSet<Entity>.ParallelWriter LeadersAssigned;

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
                    LeadersAssigned.Add(entity);
                }
            }
        }

        [WithNone(typeof(PackMember))]
        partial struct PackJoinJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public NativeArray<Entity> PackEntities;
            public ComponentLookup<Pack> PackLookup;
            public NativeParallelHashSet<Entity> LeadersAssigned;

            private void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, PassportAspect aspect)
            {
                // Skip entities that have been assigned as leaders this frame
                if (LeadersAssigned.Contains(entity)) return;

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
                if (aspect.Role != role.Role || role.QtyInfo.x <= role.QtyInfo.y)
                    return false; // Role assignment failed
                pack.MemberCount++;
                role.QtyInfo.y++;
                ecb.AddComponent(entity, new PackMember(packEntity));
                return true; // Role assignment successful

            }
        }
        
        public partial struct UpdatePackPositions : IJobEntity
        {
            [ReadOnly] public ComponentLookup<LocalToWorld> FindTransform;
            void Execute( ref LocalTransform transform, in Pack pack)
            {
                if(pack.LeaderEntity==Entity.Null) return;
                transform.Position = FindTransform[pack.LeaderEntity].Position;
                transform.Rotation = FindTransform[pack.LeaderEntity].Rotation;
            }
        }

    }
}
