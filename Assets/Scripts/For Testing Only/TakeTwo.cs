using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.AI;
namespace InfluenceMap
{
    [UpdateAfter(typeof(SetInfluences))]
    public class TakeTwo : JobComponentSystem
    {
        EntityQueryDesc Player = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Entity), typeof(PlayerCharacter), typeof(LocalToWorld) }
        };

        NativeList<Gridpoint> gridpoints = new NativeList<Gridpoint>(Allocator.TempJob);

        EntityCommandBufferSystem entityCommandBuffer;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (gridpoints.IsCreated)
                gridpoints.Dispose();

                gridpoints = new NativeList<Gridpoint>(Allocator.TempJob);
                var get = new GetGridPointWithValueOnly()
                {
                    GridpointWithValue = gridpoints
                };

                JobHandle job1 = get.Schedule(this, inputDeps);
                var playerTarget = new PlayerTargetingInfluenceJob()
                {
                    GridPointsInRange = get.GridpointWithValue,
                    PlayerCharPositions = GetComponentDataFromEntity<LocalToWorld>(true),
                    PlayerChars = GetComponentDataFromEntity<PlayerCharacter>(false),
                    entityCommandBuffer = entityCommandBuffer.CreateCommandBuffer(),
                    PlayerEntities = GetEntityQuery(Player).ToEntityArray(Allocator.TempJob)
                };
                JobHandle job2 = playerTarget.Schedule(this, job1);

                job1.Complete();
                job2.Complete();

            
            return job2;
            
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (gridpoints.IsCreated)
                gridpoints.Dispose();
        }
    }
    //Unable to burst  due to line 77
    public struct PlayerTargetingInfluenceJob : IJobForEachWithEntity<EnemyCharacter, Influencer, LookForPlayer>
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<Gridpoint> GridPointsInRange;
        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<LocalToWorld> PlayerCharPositions;
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<PlayerCharacter> PlayerChars;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> PlayerEntities;
        [NativeDisableParallelForRestriction] public EntityCommandBuffer entityCommandBuffer;

        public struct tempstruct
        {
            public Entity Player;
            public float dist;
        }

        public void Execute(Entity entity, int ThreadIndex, ref EnemyCharacter c0, ref Influencer influencer, ref LookForPlayer look)
        {


            List<tempstruct> temps = new List<tempstruct>();
            for (int index2 = 0; index2 < PlayerEntities.Length; index2++)
            {
                temps.Add(new tempstruct()
                {
                    Player = PlayerEntities[index2],
                    dist = Vector3.Distance(PlayerCharPositions[PlayerEntities[index2]].Position, PlayerCharPositions[entity].Position)
                });
            }
            temps.Sort((a, b) => (a.dist.CompareTo(b.dist)));

            // I need a way to assign NPCs to goto target and reserve space at target

            for (int index = 0; index < GridPointsInRange.Length;)
            {
                if (c0.HaveTarget)
                    goto End;
                for (int index2 = 0; index2 < temps.Count - 1; index2++)
                {
                    LocalToWorld PlayerPosition = PlayerCharPositions[temps[index2].Player];
                    float dist = Vector3.Distance(PlayerPosition.Position, GridPointsInRange[index].Position);
                    PlayerCharacter temp = PlayerChars[temps[index2].Player];

                    if (dist < 2.0f)
                    {
                        float value = GridPointsInRange[index].Player.Proximity.x - GridPointsInRange[index].Enemy.Proximity.x - temp.InfluenceInRoute;
                        if (value >= influencer.influence.Proximity.x)
                        {
                            c0.Target = temps[index2].Player;
                            temp.InfluenceInRoute += influencer.influence.Proximity.x;
                            PlayerChars[temps[index2].Player] = temp;
                            c0.gridpoint = GridPointsInRange[index];
                            entityCommandBuffer.RemoveComponent<LookForPlayer>(entity);
                            var move = new MoveToPlayer() { CanMoveToPlayer = true };
                            entityCommandBuffer.AddComponent(entity, move);
                            c0.HaveTarget = true;
                            goto End;
                        }
                    }

                }
                End: index++;
            }
        }
    }

//    // This is for Testing only This need to be tied in IAUS system once merged into base project
//    [UpdateAfter(typeof(TakeTwo))]
//    public class MoveToTargetSystem : ComponentSystem
//    {
//        EntityCommandBufferSystem entityCommandBuffer;
//        EntityQueryDesc Enemies = new EntityQueryDesc()
//        {
//            All = new ComponentType[] {typeof(EnemyCharacter), typeof(LocalToWorld) }
//        };

//        ComponentDataFromEntity<LocalToWorld> PlayerPosition;
//        protected override void OnCreate()
//        {
//            PlayerPosition = GetComponentDataFromEntity<LocalToWorld>(true);
//            entityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

//        }

//        float ResetThreshold = 5;

//        protected override void OnUpdate()
//        {
//            PlayerPosition = GetComponentDataFromEntity<LocalToWorld>(true);
//            Entities.ForEach((Entity entity, NavMeshAgent agent, ref EnemyCharacter EC, ref MoveToPlayer MP, ref Influencer influencer) =>
//            {

//               NativeList<Gridpoint> gridpoints = new NativeList<Gridpoint>(Allocator.TempJob);

//                var get = new GetGridPointWithValueOnly()
//                {
//                    GridpointWithValue = gridpoints
//                };

//                JobHandle job1 = get.Schedule(this);
//                job1.Complete();

//                var SpotCheck = new SpotCheckInfluence()
//                {
//                    GridPointsInRange = get.GridpointWithValue,
//                    entityCommandBuffer = entityCommandBuffer.CreateCommandBuffer(),
//                    PlayerChars = GetComponentDataFromEntity<PlayerCharacter>(false),
//                    PlayerPosition = GetComponentDataFromEntity<LocalToWorld>(true)

//            };

//                JobHandle job2 = SpotCheck.Schedule(this, job1);
//                job2.Complete();

//                if (EC.Target != Entity.Null)
//                {
//                    float dist = Vector3.Distance(PlayerPosition[EC.Target].Position, agent.destination);


//                    if (MP.CanMoveToPlayer)
//                    {
//                        // agent.isStopped = false;
//                        if (dist > ResetThreshold)
//                            agent.SetDestination(PlayerPosition[EC.Target].Position);
//                    }
//                    else
//                    {
//                        agent.isStopped = true;
//                    }
//                }
//                else { agent.SetDestination(Vector3.zero); }
//                gridpoints.Dispose();
//               // check.Dispose();
//            });

//        }
//        public struct SpotCheckInfluence : IJobForEachWithEntity<Influencer, MoveToPlayer,EnemyCharacter>
//        {
//          [ReadOnly][NativeDisableParallelForRestriction]public ComponentDataFromEntity<LocalToWorld> PlayerPosition;
//            [NativeDisableParallelForRestriction] public ComponentDataFromEntity<PlayerCharacter> PlayerChars;
//            [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<Gridpoint> GridPointsInRange;
//            [NativeDisableParallelForRestriction] public EntityCommandBuffer entityCommandBuffer;
//           float range { get { return 2.5f; } }
//            public void Execute(Entity entity, int indexthread, ref Influencer Inf, ref MoveToPlayer Move, ref EnemyCharacter ec)
//            {
//                if (ec.Target == Entity.Null)
//                {
//                    entityCommandBuffer.RemoveComponent<MoveToPlayer>(entity);
//                    entityCommandBuffer.AddComponent<LookForPlayer>(entity);

//                }
//                else
//                {
//                    for (int index = 0; index < GridPointsInRange.Length; index++)
//                    {
//                        float dist = Vector3.Distance(PlayerPosition[ec.Target].Position, GridPointsInRange[index].Position);

//                        if (dist < 2.0f)
//                        {


//                            float value = GridPointsInRange[index].Player.Proximity.x - GridPointsInRange[index].Enemy.Proximity.x;
//                            if (value < 0)
//                            {

//                                ec.HaveTarget = false;
//                                Vector3.Distance(PlayerPosition[ec.Target].Position, PlayerPosition[entity].Position);
//                                PlayerCharacter temp = PlayerChars[ec.Target];

//                                temp.InfluenceInRoute -= Inf.influence.Proximity.x;

//                                if (temp.InfluenceInRoute < 0)
//                                    temp.InfluenceInRoute = 0;

//                                PlayerChars[ec.Target] = temp;
//                                entityCommandBuffer.RemoveComponent<MoveToPlayer>(entity);
//                                entityCommandBuffer.AddComponent<LookForPlayer>(entity);

//                            }
//                        }
//                    }
//                }
//            }


//        }

//        }
    }
