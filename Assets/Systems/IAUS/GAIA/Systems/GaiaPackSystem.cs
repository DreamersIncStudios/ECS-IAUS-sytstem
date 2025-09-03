using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using static Unity.Entities.SystemAPI;

namespace DreamersIncStudio.GAIACollective
{
    [UpdateInGroup(typeof(GaiaUpdateGroup))]
    [UpdateAfter(typeof(GaiaSpawnSystem))]
    public partial struct GaiaPackSystem : ISystem
    {
        private EntityQuery packQuery;
        private EntityQuery packMemberQuery;
        private ComponentLookup<Pack> packLookup;
        private ComponentLookup<LocalToWorld> transformLookup;
        private BufferLookup<PackList> packListsLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            packQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                    { ComponentType.ReadOnly(typeof(LocalTransform)), ComponentType.ReadWrite(typeof(Pack)) }
            });
            packMemberQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                    { ComponentType.ReadOnly(typeof(PackMember)), ComponentType.ReadOnly<LocalToWorld>() },
            });
            packLookup = state.GetComponentLookup<Pack>();
            transformLookup = state.GetComponentLookup<LocalToWorld>(true);
            packListsLookup = state.GetBufferLookup<PackList>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var packs = packQuery.ToEntityArray(Allocator.TempJob);
            var depends = state.Dependency;
            packLookup.Update(ref state);
            transformLookup.Update(ref state);
            packListsLookup.Update(ref state);
            var packMember = packMemberQuery.ToComponentDataArray<PackMember>(Allocator.TempJob);
            var packMembersTransform = packMemberQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            var cmd = ecb.CreateCommandBuffer(state.WorldUnmanaged);
            var leaders = new NativeParallelHashSet<Entity>(packs.Length, Allocator.TempJob);

            // Combinator phase (score best leader per pack)
            var bestScores = new NativeArray<float>(packs.Length, Allocator.TempJob);
            var bestLeaders = new NativeArray<Entity>(packs.Length, Allocator.TempJob);

            // Initialize local scratch arrays on main thread
            for (int i = 0; i < packs.Length; i++)
            {
                bestScores[i] = float.NegativeInfinity;
                bestLeaders[i] = Entity.Null;
            }

            depends = new ScoreLeaders
            {
                PackEntities = packs,
                PackLookup = packLookup,
                TransformLookupRO = transformLookup,
                BestScores = bestScores,
                BestLeaders = bestLeaders
            }.Schedule(depends);

            // Commit winners (apply chosen leader to packs + mark as assigned)
            depends = new ApplyLeaders
            {
                PackEntities = packs,
                PackLookup = packLookup,
                BestLeaders = bestLeaders,
                CommandBuffer = cmd,
                LeadersAssigned = leaders,
                PackListsLookup = packListsLookup,
            }.Schedule(depends);

            // Remaining agents can join non-leader roles
            depends = new PackJoinJob()
            {
                PackEntities = packs,
                PackLookup = packLookup,
                ECB = cmd,
                LeadersAssigned = leaders,
                PackListsLookup = packListsLookup,

            }.Schedule(depends);
            
            depends = new UpdatePackPositions()
            {
                FindTransform = transformLookup
            }.Schedule(depends);
            depends = new UpdatePackCenter()
            {
                PackMembersTransform = transformLookup
                
            }.Schedule(depends);
            depends = packs.Dispose(depends);
            depends = bestScores.Dispose(depends);
            depends = bestLeaders.Dispose(depends);
            depends = leaders.Dispose(depends);
            depends = packMember.Dispose(depends);
            depends = packMembersTransform.Dispose(depends);
            state.Dependency = depends;
        }

     
        [WithNone(typeof(PackMember))]
        partial struct ScoreLeaders : IJobEntity
        {
            public NativeArray<Entity> PackEntities;
            public ComponentLookup<Pack> PackLookup;
            [ReadOnly] public ComponentLookup<LocalToWorld> TransformLookupRO;

            // Scratch outputs (per-pack)
            public NativeArray<float> BestScores;
            public NativeArray<Entity> BestLeaders;

            private void Execute(Entity candidate, in LocalToWorld transfom, PassportAspect aspect)
            {
                // Candidate must be able to lead its matching pack role
                for (int i = 0; i < PackEntities.Length; i++)
                {
                    var packEntity = PackEntities[i];
                    var pack = PackLookup[packEntity];
                    if (pack.LeaderEntity != Entity.Null) continue;
                    if (pack.Role != aspect.Role) continue;
                    
                    //  scoring
                    var distance = math.distance(transfom.Position, TransformLookupRO[packEntity].Position);
                    var distanceScore = DistanceInverse(distance, 0.001f); // Avoid div-by-zero
                    var levelScore = LevelScore(aspect.Level);

                    var score = CombineWeighted(distanceScore, 0.7f, levelScore, 0.3f);

                    // Keep best per pack (single-thread scheduled => no atomics needed)
                    if (!(score > BestScores[i])) continue;
                    BestScores[i] = score;
                    BestLeaders[i] = candidate;
                }
            }

      
            private static float DistanceInverse(float d, float epsilon) => 1f / math.max(d, epsilon);
            private static float LevelScore(int level) => level; // Identity; replace with normalization if needed
            private static float CombineWeighted(float a, float wa, float b, float wb) => a * wa + b * wb;
        }

        private struct ApplyLeaders : IJob
        {
            public NativeArray<Entity> PackEntities;
            public ComponentLookup<Pack> PackLookup;
            [ReadOnly] public NativeArray<Entity> BestLeaders;
            public EntityCommandBuffer CommandBuffer;
            public NativeParallelHashSet<Entity> LeadersAssigned;
            public BufferLookup<PackList> PackListsLookup;

            public void Execute()
            {
                for (int i = 0; i < PackEntities.Length; i++)
                {
                    var packEntity = PackEntities[i];
                    var pack = PackLookup[packEntity];
                    if (pack.LeaderEntity != Entity.Null) continue;

                    var winner = BestLeaders[i];
                    if (winner == Entity.Null) continue;

                    pack.LeaderEntity = winner;
                    pack.MemberCount++;
                    CommandBuffer.AddComponent(winner, new PackMember(packEntity));
                    PackLookup[packEntity] = pack;
                    PackListsLookup[packEntity].Add(new PackList(winner, pack.Role));//add leader role
                    LeadersAssigned.Add(winner);
                }
            }
        }


        [WithNone(typeof(PackMember))]
        private partial struct PackJoinJob : IJobEntity
        {
            public EntityCommandBuffer ECB;
            public NativeArray<Entity> PackEntities;
            public ComponentLookup<Pack> PackLookup;
            public BufferLookup<PackList> PackListsLookup;
            public NativeParallelHashSet<Entity> LeadersAssigned;

            private void Execute(Entity entity, PassportAspect aspect)
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
                    PackListsLookup[packEntity].Add(new PackList {PackMember = packEntity,MemberRole = aspect.Role});
                    PackLookup[packEntity] = pack; // Save the updated pack
                }
            }

            private bool TryAssignRole(
                Entity entity,
                PassportAspect aspect,
                ref PackRole role,
                Entity packEntity,
                ref Pack pack)
            {
                if (aspect.Role != role.Role || role.QtyInfo.x <= role.QtyInfo.y)
                    return false;
                pack.MemberCount++;
                role.QtyInfo.y++;
                ECB.AddComponent(entity, new PackMember(packEntity));
                return true;
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
        
        public partial struct UpdatePackCenter : IJobEntity
        {
            [ReadOnly] public ComponentLookup<LocalToWorld> PackMembersTransform;
            void Execute( Entity entity,ref Pack pack, DynamicBuffer<PackList> packLists)
            {
                if(pack.LeaderEntity==Entity.Null) return;
                int cnt = 0;
                float3 center = float3.zero;
                foreach (var member in packLists)
                {
                    cnt++;
                    center += PackMembersTransform[member.PackMember].Position; 
                }
                pack.HerdCenter = center / cnt;
            }
        }
    }
}