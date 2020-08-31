
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Components.MovementSystem;
using Unity.Mathematics;
using IAUS.Core;
using Utilities;
using Unity.Collections;
using Unity.Burst;
using Stats;

namespace IAUS.ECS2
{
    [System.Serializable]
    [GenerateAuthoringComponent]
    public struct FollowCharacter : BaseStateScorer
    {
        public Entity Target;

        public float DistanceToMantainFromTarget;
        // do we need a enemy in range consideration?
        public ConsiderationData Health;

        public float3 TargetLocation;
        public float DistanceAtStart;
        public bool IsTargetMoving;
        public float mod { get { return 1.0f - (1.0f / 2.0f); } }

        [SerializeField] ActionStatus _status;
        [SerializeField] public float _resetTimer;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;

        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float ResetTimer { get { return _resetTimer; } set { _resetTimer = value; } }
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
    }

    public struct Squad : IComponentData
    {
        public int TeamMemberNumber;
    }
    public struct getpointTag : IComponentData { }

    [UpdateInGroup(typeof(IAUS_UpdateScore))]
    public class FollowStateScore :SystemBase
    {
        protected override void  OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = Entities.ForEach((ref FollowCharacter follow, in PlayerStatComponent stats) =>
            {
                float targetcheck = follow.IsTargetMoving ? 1.0f : 0.0f;

                float TotalScore = Mathf.Clamp01(follow.Health.Output(stats.HealthRatio)
                    * targetcheck
                    )
                ;
                follow.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * follow.mod) * TotalScore);

            }).ScheduleParallel(systemDeps);
            Dependency = systemDeps;
        }
    }


    [UpdateInGroup(typeof(IAUS_UpdateState))]
    [UpdateBefore(typeof(PatrolAction))]
    public class FollowPosition : ComponentSystem
    {

        protected override void OnUpdate()
        {
       // ComponentDataFromEntity<Patrol> patrol = GetComponentDataFromEntity<Patrol>(true);

            Entities
                .ForEach((Entity entity, ref FollowCharacter Follow, ref getpointTag tag) =>
            {
                Patrol patrol = EntityManager.GetComponentData<Patrol>(Follow.Target);
                Vector3 tempPoint = new Vector3();
                float3 Center = patrol.waypointRef;
                retry:
                if (GlobalFunctions.RandomPoint(Center, 10, out tempPoint))
                {
                    Follow.TargetLocation = tempPoint;
                }
                else
                {
                    goto retry;
                }
                EntityManager.RemoveComponent<getpointTag>(entity);
            });
        }
    }
    
    public struct FollowTargetTag : IComponentData
    {
        bool test;
    }

  
    [UpdateInGroup(typeof(IAUS_UpdateState))]
    [UpdateAfter(typeof(FollowPosition))]

    public class FollowAction : SystemBase
    {
        EntityCommandBufferSystem _entityCommandBufferSystem;
        private EntityQuery _followCharacterQuery;
        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _followCharacterQuery = GetEntityQuery(new EntityQueryDesc() {
                All = new ComponentType[] {ComponentType.ReadWrite(typeof(FollowCharacter)),ComponentType.ReadWrite(typeof(Movement)), /* ComponentType.ReadWrite(typeof(InfluenceValues)),*/
                    ComponentType.ReadOnly(typeof(FollowTargetTag)),ComponentType.ReadOnly(typeof(BaseAI)) }
            });

        }
        protected override void OnUpdate( )
        {
            JobHandle systemDeps = Dependency;
                
            systemDeps = new FollowCharacterActionJob()
            {
                EntityChunk=GetArchetypeChunkEntityType(),
                FollowChunk = GetArchetypeChunkComponentType<FollowCharacter>(false),
                MovementChunk = GetArchetypeChunkComponentType<Movement>(false),
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }
            .ScheduleParallel(_followCharacterQuery, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }
    }

    /// <summary>
    /// Influnces code need to be write for group of followers ??
    /// </summary>
    [BurstCompile]
    public struct FollowCharacterActionJob : IJobChunk
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
       [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
        public ArchetypeChunkComponentType<FollowCharacter> FollowChunk;
        public ArchetypeChunkComponentType<Movement> MovementChunk;
      //  public ArchetypeChunkComponentType<InfluenceValues> InfluenceChunk;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<FollowCharacter> follows = chunk.GetNativeArray<FollowCharacter>(FollowChunk);
            NativeArray<Movement> moves = chunk.GetNativeArray<Movement>(MovementChunk);
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                FollowCharacter follow = follows[i];
                Movement move = moves[i];
                Entity entity = entities[i];

                if (follow.Status == ActionStatus.Success)
                {
                    entityCommandBuffer.RemoveComponent < FollowTargetTag>(chunkIndex, entity);
                    return;
                }


                if (!follow.TargetLocation.Equals(move.TargetLocation)) 
                {
                    move.TargetLocation = follow.TargetLocation;
                    move.SetTargetLocation = true;
                    follow.Status = ActionStatus.Running;
                    move.Completed = false;
                    move.CanMove = true;
                }

                //complete
                if (follow.Status == ActionStatus.Running)
                {
                    if (move.Completed && !move.CanMove)
                    {
                        follow.Status = ActionStatus.Success;
                    }
                }


                follows[i] = follow;
                moves[i] = move;
            }
        }
    }


}


