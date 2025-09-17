using AISenses;
using Components.MovementSystem;
using DreamersInc.InfluenceMapSystem;
using IAUS.ECS.Component;
using Stats.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;
using Utilities;
using RaycastHit = Unity.Physics.RaycastHit;

namespace IAUS.ECS.Systems
{
    public partial struct TerrorizeAreaSystem : ISystem
    {
        private EntityQuery terrorizableObjectQuery;
        private BeginSimulationEntityCommandBufferSystem.Singleton ecb;
        private ComponentLookup<AIStat> aiStatLookup;
        private ComponentLookup<LocalToWorld> transformLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.EntityManager.CompleteDependencyBeforeRO<PhysicsWorldSingleton>();
            state.RequireForUpdate<AIStat>();
            terrorizableObjectQuery = SystemAPI.QueryBuilder()
                .WithAll<LocalToWorld,InfluenceComponent,AIStat>()
              //  .WithAny<Generator>()
                .Build();
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
            foreach (var (aspect,move,local) in SystemAPI.Query<RefRW<TerrorizeAreaState>,Movement,LocalToWorld>().WithAll<TerrorizeAreaTag>().WithNone<ManualControlIAUS>())
            {
                if (aspect.ValueRO.AttackPlans.IsEmpty) continue;
                if (aspect.ValueRO.AttackPlans[0] != AttackPlan.Wander) continue;
                if (!GlobalFunctions.RandomPoint(local.Position, 50, out float3 point)) continue;
               move.SetLocation(point); 
                aspect.ValueRW.AttackPlans.RemoveAt(0);
            }
            
            depends = new FindObjectToTerrorize()
            {
                InteractionPositions = terrorizableObjectQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
                InteractionStats = terrorizableObjectQuery.ToComponentDataArray<AIStat>(Allocator.TempJob),
                InteractionEntities = terrorizableObjectQuery.ToEntityArray(Allocator.TempJob),
                AIStatLookup = this.aiStatLookup,
                World = world
            }.Schedule(depends);
            depends = new DetermineAction()
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                ECB = ecb.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
                
            }.Schedule(depends);
            depends = new GetTerrorizeLocation()
            {
                TransformLookup = this.transformLookup
            }.Schedule(depends);
            state.Dependency = depends;
        }

        [BurstCompile]
        private partial struct FindObjectToTerrorize : IJobEntity
        {
            public NativeArray<LocalToWorld> InteractionPositions;
            public NativeArray<AIStat> InteractionStats;
            public NativeArray<Entity> InteractionEntities;
           [ReadOnly] public ComponentLookup<AIStat> AIStatLookup;
           [ReadOnly] public CollisionWorld World;
            void Execute(Entity entity, ref TerrorizeAreaState state, in LocalToWorld transform, in Vision vision)
            {
                //Todo add influence check  
                if(state.TargetEntity != Entity.Null && AIStatLookup[state.TargetEntity].HealthRatio>.25f)
                    return;
                state.TargetEntity = Entity.Null;
                
                var MAX = float.MaxValue;
                var indexof = -1;
                for (int i = 0; i < InteractionPositions.Length; i++)
                {
                    var distance = Vector3.Distance(transform.Position, InteractionPositions[i].Position);
                    if (distance > vision.ViewRadius || distance > MAX || InteractionStats[i].HealthRatio<.25f) continue;
                    var dirToTarget = ((Vector3)InteractionPositions[i].Position -
                                       (Vector3)(transform.Position + new float3(0, 1, 0))).normalized;
                    if (!(Vector3.Angle(transform.Forward, dirToTarget) < vision.ViewAngle / 2.0f)) continue;
                    var raycastInput = new RaycastInput()
                    {
                        Start = transform.Position + new float3(0, 1, 0) + transform.Forward * 3f,
                        End = InteractionPositions[i].Position + new float3(0, 1, 0),
                        Filter = new CollisionFilter()
                        {
                            BelongsTo = ((1 << 10)),
                            CollidesWith = PhysicsCategoryTags.Everything.Value,
                            GroupIndex = 0
                        }
                    };
                    if (!World.CastRay(raycastInput, out RaycastHit raycastHit)) continue;
                    if (!raycastHit.Entity.Equals(InteractionEntities[i])) continue;
                    MAX = distance;
                    indexof = i;
                }

                if (indexof == -1) return;
                state.TargetEntity = InteractionEntities[indexof];
            }
        }

        [BurstCompile]
        private partial struct GetTerrorizeLocation: IJobEntity
        {

            [ReadOnly] public ComponentLookup<LocalToWorld> TransformLookup;

            void Execute(Entity entity, ref TerrorizeAreaState state)
            {
                if (state.TargetEntity == Entity.Null) return;
                var transform = TransformLookup[state.TargetEntity];
                state.TargetPosition = transform.Position;
            }
        }

        partial struct DetermineAction: IJobEntity
        {
            public float deltaTime;
            public EntityCommandBuffer.ParallelWriter ECB;
         
            void Execute([ChunkIndexInQuery]int chunkIndex, Entity entity, TerrorizeAspect aspect,  in TerrorizeAreaTag tag)
            {
                if (aspect.Plan.IsEmpty)
                {
                    aspect.DeterminePlan();
                }

                aspect.ExecutePlan(entity, chunkIndex, deltaTime,ECB);
            }
        }

    }
}