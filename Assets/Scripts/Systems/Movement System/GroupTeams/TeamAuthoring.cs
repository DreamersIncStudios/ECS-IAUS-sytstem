using System.Collections;
using System.Collections.Generic;
using Unity.Transforms;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
namespace Dreamers.SquadSystem
{

    public class TeamAuthoring : MonoBehaviour,IConvertGameObjectToEntity
    {
        public TeamInfo Info;
        public bool IsLeader;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (IsLeader)
            {
                dstManager.AddComponentData(entity, Info);
            }
            dstManager.AddBuffer<Team>(entity);
        }

    }

    public class TeamUpSystem : SystemBase
    {
        EntityQuery Leaders;
        EntityQuery Grunts;
        EntityQuery TeamedGrunts;

        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            Leaders = GetEntityQuery(new EntityQueryDesc() {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(LocalToWorld)),ComponentType.ReadOnly(typeof(LeaderEntityTag)),
                 ComponentType.ReadOnly(typeof(Team))  
                }

            });

            Grunts = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(LocalToWorld)),ComponentType.ReadOnly(typeof(GruntEntityTag)),
                 ComponentType.ReadOnly(typeof(Team))
                }
            });
            TeamedGrunts = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(LocalToWorld)),ComponentType.ReadOnly(typeof(GruntEntityTag)),
                 ComponentType.ReadOnly(typeof(Team)), ComponentType.ReadOnly(typeof(updateDataTeam))
                }
            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            NativeArray<LocalToWorld> GPos = Grunts.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            NativeArray<Entity> GEntities = Grunts.ToEntityArray(Allocator.TempJob);

            systemDeps = new TeamUP() 
            {
                TeamInfoChunk = GetComponentTypeHandle<TeamInfo>(false),
             //   TeamBuffer = GetBufferTypeHandle<Team>(false),
                Entities = GetEntityTypeHandle(),
                TransfomsChunk = GetComponentTypeHandle<LocalToWorld>(true),
                GruntsTags = GetComponentDataFromEntity<GruntEntityTag>(false),
                Grunts = GEntities,
                GruntPositions = GPos,
                GruntsBuffers = GetBufferFromEntity<Team>(false),
                ECB = _entityCommandBufferSystem.CreateCommandBuffer()
            }.ScheduleSingle(Leaders, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new UpdateTeamList() {
                TeamBuffer = GetBufferFromEntity<Team>(false),
                EntityChunk = GetEntityTypeHandle(),
                ECB = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter()
            }.ScheduleParallel(TeamedGrunts, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;

        }
    }
    [Unity.Burst.BurstCompile]
    public struct TeamUP : IJobChunk
    {
         public ComponentTypeHandle<TeamInfo> TeamInfoChunk;
        [ReadOnly] public EntityTypeHandle Entities;
        [ReadOnly] public ComponentTypeHandle<LocalToWorld> TransfomsChunk;
        [ReadOnly][DeallocateOnJobCompletion] public NativeArray<LocalToWorld> GruntPositions;
       [NativeDisableParallelForRestriction] public ComponentDataFromEntity<GruntEntityTag> GruntsTags;
       [DeallocateOnJobCompletion] public NativeArray<Entity> Grunts;
        [NativeDisableParallelForRestriction]public BufferFromEntity<Team> GruntsBuffers;
        public EntityCommandBuffer ECB;


     //   public BufferTypeHandle<Team> TeamBuffer;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            if (Grunts.Length == 0)
                return;
            NativeArray<Entity> entities = chunk.GetNativeArray(Entities);
            NativeArray<LocalToWorld> transforms = chunk.GetNativeArray(TransfomsChunk);
            NativeArray<TeamInfo> TeamInfos = chunk.GetNativeArray(TeamInfoChunk);
                
          //  BufferAccessor<Team> buffer = chunk.GetBufferAccessor(TeamBuffer);

            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entities[i];
                TeamInfo Info = TeamInfos[i];
                LocalToWorld transform = transforms[i];
                DynamicBuffer<Team> TeamList = GruntsBuffers[entity];

                for (int j = 0; j < Grunts.Length; j++)
                {
                    if (Info.IsFilled)
                    {
                        if (!GruntsTags[Grunts[j]].HasSquad)
                        {
                            float distToLeader = Vector3.Distance(GruntPositions[j].Position, transform.Position);
                            if (distToLeader < 30) // change to variable later
                            {
                                // add to squad and update others 

                                DynamicBuffer<Team> gruntBuffer = GruntsBuffers[Grunts[j]];
                                gruntBuffer.Add(new Team()
                                {
                                    HasLeader = true,
                                    IsLeader = true,
                                    TeamMember = entity
                                });

                                TeamList.Add(new Team()
                                {
                                    HasLeader = true,
                                    IsLeader = false,
                                    TeamMember = Grunts[j]
                                });
                                ECB.AddComponent<updateDataTeam>(Grunts[j]);
                                Info.CurrentTeamSize++;
                                GruntEntityTag tag = GruntsTags[Grunts[j]];
                                tag.HasSquad = true;
                                GruntsTags[Grunts[j]] = tag;
                            }

                        }
                    }
                }

                TeamInfos[i] = Info;
            }

        }
    }

    [Unity.Burst.BurstCompile]
    public struct UpdateTeamList : IJobChunk
    {
        [NativeDisableParallelForRestriction]public BufferFromEntity<Team> TeamBuffer;
       [ReadOnly] public EntityTypeHandle EntityChunk;
        public EntityCommandBuffer.ParallelWriter ECB;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entities[i];
                DynamicBuffer<Team> team = TeamBuffer[entity];
                Entity leader = new Entity();

                for (int j = 0; j < team.Length; j++)
                {
                    if (team[j].IsLeader) {
                        leader = team[j].TeamMember;
                    }
                }

                team.Clear();
                team.Add(new Team() {
                    TeamMember = leader,
                    IsLeader = true,
                    HasLeader = true
                });
                DynamicBuffer<Team> CopyThis = TeamBuffer[leader];
                
                for (int j = 0; j < CopyThis.Length; j++)
                {
                    if (!entity.Equals(CopyThis[j].TeamMember)) {
                        team.Add(CopyThis[j]);
                    }

                }
                ECB.RemoveComponent<updateDataTeam>(chunkIndex,entity);

                // add all other team entries
            }

        }
    }
}