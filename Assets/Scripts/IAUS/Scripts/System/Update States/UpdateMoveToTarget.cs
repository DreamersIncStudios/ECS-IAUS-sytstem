using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS.Component;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;
using Stats;
using Unity.Physics;
using Unity.Physics.Systems;
using AISenses;
using Components.MovementSystem;


namespace IAUS.ECS.Systems
{

    public class UpdateMoveToTarget : SystemBase
    {
        private EntityQuery Movers;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            Movers = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(MoveToTarget)), ComponentType.ReadOnly(typeof(FollowEntityTag)), ComponentType.ReadOnly(typeof(LocalToWorld)),
                    ComponentType.ReadOnly(typeof(CharacterStatComponent)), ComponentType.ReadWrite(typeof(AttackTargetState)), ComponentType.ReadWrite(typeof(Movement))

                }
            });
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            systemDeps = new CheckDistanceToLeader()
            {
                MoveChunk = GetComponentTypeHandle<MoveToTarget>(false),
                FollowerChunk = GetComponentTypeHandle<FollowEntityTag>(true),
                PositionChunk = GetComponentTypeHandle<LocalToWorld>(true),
                EntityPositions = GetComponentDataFromEntity<LocalToWorld>(),
                AttackChunk = GetComponentTypeHandle<AttackTargetState>(false)
            }.ScheduleParallel(Movers, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);


            systemDeps = new ScoreMoveState()
            {
                MoveChunk = GetComponentTypeHandle<MoveToTarget>(false),
                StatsChunk = GetComponentTypeHandle<CharacterStatComponent>(true),
                AttackChunk = GetComponentTypeHandle<AttackTargetState>(true)

            }.ScheduleParallel(Movers, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            if (UnityEngine.Time.frameCount % 360 == 2) {
                systemDeps = new CheckIfTargetIsStillInSightandUpdate()
                { 
                    EntityPositions = GetComponentDataFromEntity<LocalToWorld>(true),
                    MoveChunk = GetComponentTypeHandle<MoveToTarget>(false),
                    SeersPositionChunk = GetComponentTypeHandle<LocalToWorld>(true),
                    MovementChunk= GetComponentTypeHandle<Movement>(false),
                    physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld
                }.ScheduleParallel(Movers, systemDeps);

                systemDeps.Complete();

                _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            }


            Dependency = systemDeps;
        }

        [BurstCompile]
        struct CheckDistanceToLeader : IJobChunk
        {
            public ComponentTypeHandle<MoveToTarget> MoveChunk;
            public ComponentTypeHandle<AttackTargetState> AttackChunk;

            [ReadOnly] public ComponentTypeHandle<FollowEntityTag> FollowerChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> PositionChunk;
            [ReadOnly] [NativeDisableParallelForRestriction] public ComponentDataFromEntity<LocalToWorld> EntityPositions;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<MoveToTarget> Moves = chunk.GetNativeArray(MoveChunk);
                NativeArray<LocalToWorld> Positions = chunk.GetNativeArray(PositionChunk);
                NativeArray<FollowEntityTag> Followers = chunk.GetNativeArray(FollowerChunk);
                NativeArray<AttackTargetState> Attack = chunk.GetNativeArray(AttackChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    MoveToTarget move = Moves[i];
                    AttackTargetState attack = Attack[i];

                    move.DistanceToLeader = Vector3.Distance(Positions[i].Position, EntityPositions[Followers[i].Leader].Position);
                    if (move.HasTarget)
                    {
                        attack.DistanceToTarget = Vector3.Distance(Positions[i].Position, EntityPositions[move.Target.target.entity].Position);
                        attack.Target = move.Target.target.entity;
                    }
                    else
                        attack.DistanceToTarget = 0.0f;

                    move.InRange = attack.InRangeForAttack;
                    Moves[i] = move;
                    Attack[i] = attack;
                }
            }
        }

        [BurstCompile]
        public struct ScoreMoveState : IJobChunk
        {
            public ComponentTypeHandle<MoveToTarget> MoveChunk;
            [ReadOnly] public ComponentTypeHandle<AttackTargetState> AttackChunk;
            [ReadOnly] public ComponentTypeHandle<CharacterStatComponent> StatsChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<MoveToTarget> Moves = chunk.GetNativeArray(MoveChunk);
                NativeArray<CharacterStatComponent> Stats = chunk.GetNativeArray(StatsChunk);
                NativeArray<AttackTargetState> Attack = chunk.GetNativeArray(AttackChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    MoveToTarget move = Moves[i];
                    CharacterStatComponent stats = Stats[i];
                    AttackTargetState attack = Attack[i];
                    if (move.HasTarget)
                    {

                        float TotalScore = move.DistanceToLead.Output(move.DistanceRatio) * move.HealthRatio.Output(stats.HealthRatio) * (attack.AttackRange = true ? .15f : 1.0f);
                        move.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * move.mod) * TotalScore);
                    }
                    else
                    { move.TotalScore = 0.0f; }
                    Moves[i] = move;

                }

            }
        }

       // [BurstCompile]
        public struct CheckIfTargetIsStillInSightandUpdate : IJobChunk
        {
            public ComponentTypeHandle<MoveToTarget> MoveChunk;
            [ReadOnly] [NativeDisableParallelForRestriction] public ComponentDataFromEntity<LocalToWorld> EntityPositions;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> SeersPositionChunk;
            public ComponentTypeHandle<Movement> MovementChunk;

           [ReadOnly] public PhysicsWorld physicsWorld;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<MoveToTarget> Moves = chunk.GetNativeArray(MoveChunk);
                NativeArray<LocalToWorld> SeersPosition = chunk.GetNativeArray(SeersPositionChunk);
                CollisionWorld collisionWorld = physicsWorld.CollisionWorld;
                NativeArray<Movement> movements = chunk.GetNativeArray(MovementChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    MoveToTarget move = Moves[i];
                    if (!move.HasTarget)
                    { continue; }

                    LocalToWorld seerPosition = SeersPosition[i];
                    Movement mover = movements[i];

                    ScanPositionBuffer update = new ScanPositionBuffer();
                    update.target.entity = move.Target.target.entity;
                    update.target.LastKnownPosition = EntityPositions[move.Target.target.entity].Position;
                    update.target.LookAttempt = move.Target.target.LookAttempt;
                    //Make rays 
                    RaycastInput raycastCenter = new RaycastInput()
                    {
                        Start = seerPosition.Position,
                        End = update.target.LastKnownPosition,
                        Filter = new CollisionFilter
                        {
                            BelongsTo = ~0u,
                            CollidesWith = ((1 << 10) | (1 << 11) | (1 << 12)),
                            GroupIndex = 0
                        }
                    };
                    // cast rays

                    if (collisionWorld.CastRay(raycastCenter, out Unity.Physics.RaycastHit raycastHit))
                    {
                        float dist = Vector3.Distance(seerPosition.Position, update.target.LastKnownPosition);

                        update.target.DistanceTo = raycastHit.Fraction * dist;
                        update.target.LastKnownPosition = raycastHit.Position;
                        update.target.CanSee = true;
                        update.target.LookAttempt = 0;
                    }
                    else { 
                        update.target.CanSee = false;
                        update.target.LookAttempt++;
                    }
                    if (update.target.CantFind) {
                        // Replace with possible look for target AI State???????
                        update = new ScanPositionBuffer() { target = new Target() { CanSee = false } };
                    }
                    move.Target = update;
                    if (update.target.CanSee && !update.target.LastKnownPosition.Equals(mover.TargetLocation))
                    {
                        mover.TargetLocation = update.target.LastKnownPosition;
                        mover.CanMove = true;
                        mover.SetTargetLocation = true;

                    }

                    Moves[i] = move;
                    movements[i] = mover;

                }
            }

        }
    }



    
}
