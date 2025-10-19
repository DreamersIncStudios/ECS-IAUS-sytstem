using System.Collections.Generic;
using AISenses;
using Combinators.Interactable;
using Components.MovementSystem;
using IAUS.ECS.Component;
using Stats.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Utilities;

namespace IAUS.ECS.Systems
{
    public partial struct TerrorizeAreaSystem : ISystem
    {
        private BeginSimulationEntityCommandBufferSystem.Singleton ecb;
        private ComponentLookup<AIStat> aiStatLookup;
        private ComponentLookup<LocalToWorld> transformLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.EntityManager.CompleteDependencyBeforeRO<PhysicsWorldSingleton>();
            state.RequireForUpdate<AIStat>();

            ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            aiStatLookup = state.GetComponentLookup<AIStat>(true);
            transformLookup = state.GetComponentLookup<LocalToWorld>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            var world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            world.UpdateBodyIndexMap();

            aiStatLookup.Update(ref state);
            transformLookup.Update(ref state);

            var depends = state.Dependency;
            foreach (var (aspect, move, local) in SystemAPI.Query<RefRW<TerrorizeAreaState>, Movement, LocalToWorld>()
                         .WithAll<TerrorizeAreaTag>().WithNone<ManualControlIAUS>())
            {
                if (aspect.ValueRO.AttackPlans.IsEmpty) continue;
                if (aspect.ValueRO.AttackPlans[0] != AttackPlan.Wander) continue;
                if (!GlobalFunctions.RandomPoint(local.Position, 50, out float3 point)) continue;
                move.SetLocation(point);
                aspect.ValueRW.AttackPlans.RemoveAt(0);
            }

            depends = new FindObjectToTerrorize()
            {
            }.Schedule(depends);
            depends = new GetTerrorizeLocation()
            {
                TransformLookup = this.transformLookup
            }.Schedule(depends);
            // depends = new DetermineAction()
            // {
            //     deltaTime = SystemAPI.Time.DeltaTime,
            //     ECB = ecb.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            // }.Schedule(depends);
            //
            state.Dependency = depends;
        }

        [BurstCompile]
        private partial struct FindObjectToTerrorize : IJobEntity
        {
            void Execute(Entity entity, ref TerrorizeAreaState state, in LocalToWorld transform,
                DynamicBuffer<PlacesOfInterest> placesOfInterest, DynamicBuffer<AISenses.Resources> resources)
            {
                List<Target> targets = new List<Target>();
                foreach (var place in placesOfInterest)
                {
                    targets.Add(place.Target);
                }

                foreach (var resource in resources)
                {
                    targets.Add(resource.Target);
                }

                var targetsInRange = PredChain
                    .Start(new FilterDuplicates())
                    .And(new IsAttackable())
                    .And(new SortByRange())
                    .And(new SortByInfluence())
                    .Build();
                var searchCTX = new InteractableCTX(transform.Position, transform.Forward);
                var filtered = targetsInRange.Test(targets, searchCTX);
                if (filtered.Count == 0) return;
                state.TargetEntity = filtered[0].Entity;
                Debug.Log("Have target to terrorize");
            }
        }

        [BurstCompile]
        private partial struct GetTerrorizeLocation : IJobEntity
        {
            [ReadOnly] public ComponentLookup<LocalToWorld> TransformLookup;

            void Execute(Entity entity, ref TerrorizeAreaState state)
            {
                if (state.TargetEntity == Entity.Null) return;
                var transform = TransformLookup[state.TargetEntity];
                state.TargetPosition = transform.Position;
            }
        }

        partial struct DetermineAction : IJobEntity
        {
            public float deltaTime;
            public EntityCommandBuffer.ParallelWriter ECB;

            void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, TerrorizeAspect aspect,
                in TerrorizeAreaTag tag)
            {
                if (aspect.CurAttackStep == AttackPlan.None)
                    aspect.DeterminePlan();

                aspect.ExecutePlan(entity, chunkIndex, deltaTime, ECB);
            }
        }
    }
}
