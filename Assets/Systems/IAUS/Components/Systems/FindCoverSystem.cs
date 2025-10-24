using System.Collections.Generic;
using Combinators.Attack_AI;
using Global.Component;
using IAUS.ECS.Component;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.ECS.Systems
{
    [UpdateInGroup(typeof(AISenses.VisionSystems.VisionTargetingUpdateGroup))]
    public partial struct FindCoverSystem : ISystem
    {
        EntityQuery coverQuery;
        ComponentLookup<AITarget> lookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.EntityManager.CompleteDependencyBeforeRO<PhysicsWorldSingleton>();

            coverQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                    { ComponentType.ReadOnly(typeof(Cover)), ComponentType.ReadOnly(typeof(LocalToWorld)) }
            });
            lookup = state.GetComponentLookup<AITarget>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            var depends = state.Dependency;
        lookup.Update(ref state);
            state.EntityManager.CompleteDependencyBeforeRO<PhysicsWorldSingleton>();
            var world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            depends = new CoverJobAttack()
            {
                Covers = coverQuery.ToComponentDataArray<Cover>(Allocator.TempJob),
                AITargetInfo = lookup,
                CoversPositions = coverQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
                World = world
            }.Schedule(depends);
            depends = new CoverJobTerror()
            {
                AITargetData =  lookup,
                Covers = coverQuery.ToComponentDataArray<Cover>(Allocator.TempJob),
                CoversPositions = coverQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
                World = world
            }.Schedule(depends);


            state.Dependency = depends;
        }

        partial struct CoverJobAttack : IJobEntity
        {
            [ReadOnly] public CollisionWorld World;
            [ReadOnly] public NativeArray<Cover> Covers;
            [ReadOnly] public NativeArray<LocalToWorld> CoversPositions;
            [ReadOnly]  public ComponentLookup<AITarget> AITargetInfo;
            void Execute(AttackActionTag aspect, in LocalToWorld transform, in PhysicsInfo physicsInfo)
            {
                var offset = AITargetInfo[aspect.TargetEntity].CenterOffset;
                var ctx = new PositionCTX(transform.Position, transform.Forward, 75,
                    aspect.TargetPosition, offset, aspect.TargetEntity, World,
                    new CollisionFilter()
                    {
                        BelongsTo = ((1 << 11)),
                        CollidesWith = physicsInfo.CollidesWith.Value,
                        GroupIndex = 0
                    });

                var coverData = new List<CoverData>();

                for (int i = 0; i < Covers.Length; i++)
                {
                    coverData.Add(new CoverData(CoversPositions[i].Position, Covers[i]));
                }

                var coverInRange = PredChain.Start(new InEffectiveAttackRange())
                    .And(new CanSeeTargetStage())
                    .Build();
                var ans = coverInRange.Test(coverData, in ctx);
            }
        }

        partial struct CoverJobTerror : IJobEntity
        {
            [ReadOnly] public CollisionWorld World;
            [ReadOnly] public NativeArray<Cover> Covers;
            [ReadOnly] public NativeArray<LocalToWorld> CoversPositions;
            [ReadOnly]  public ComponentLookup<AITarget> AITargetData;
            void Execute(TerrorizeAreaTag aspect, in LocalToWorld transform, in PhysicsInfo physicsInfo)
            {
                var offset = AITargetData[aspect.TargetEntity].CenterOffset;
                var ctx = new PositionCTX(transform.Position, transform.Forward, 75,
                    aspect.TargetPosition, offset, aspect.TargetEntity, World,
                    new CollisionFilter()
                    {
                        BelongsTo = ((1 << 11)),
                        CollidesWith = physicsInfo.CollidesWith.Value,
                        GroupIndex = 0
                    });
        
                var coverData = new List<CoverData>();
                for (int i = 0; i < Covers.Length; i++)
                {
                    coverData.Add(new CoverData(CoversPositions[i].Position, Covers[i]));
                }

                var coverInRange = PredChain.Start(new InEffectiveAttackRange())
                    .And(new CanSeeTargetStage())
                    .Build();
                var ans = coverInRange.Test(coverData, in ctx);
                Debug.Log(ans.Count);
            }
        }
    }
}