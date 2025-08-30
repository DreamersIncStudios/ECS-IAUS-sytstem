using DreamersInc.InfluenceMapSystem;
using Global.Component;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Utilities;
using RaycastHit = Unity.Physics.RaycastHit;



namespace AISenses.VisionSystems
{
    public struct MapVision : IComponentData
    {

        public float3x4 CoverPositions; // nearest, Farthest, Lowest threat but in range, random location in range 

    }

    public partial class MapVisionSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<RunningTag>();
            RequireForUpdate<PhysicsWorldSingleton>();
            EntityManager.CompleteDependencyBeforeRO<PhysicsWorldSingleton>();
        }

        protected override void OnUpdate()
        {

            collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            Entities.ForEach((ref MapVision mapVision, ref EvadeThreat state, ref LocalTransform transform) =>
                {
                    mapVision.CoverPositions.c0 = ClosestSafeLocation(mapVision, state,transform);
                    mapVision.CoverPositions.c1 = FarthestSafeLocation(ref mapVision, ref state, transform);
                }).WithoutBurst()
                .Run();
        }

        private CollisionWorld collisionWorld;
        
        /// <summary>
        /// Determines the target position for a Magic attack by looking for cover position with line of sight
        /// </summary>
        /// <param name="mapVision">The MapVision component containing possible position for NPC to go to .</param>
        /// <param name="state">The AttackState component representing the attack state.</param>
        /// <returns>The target position for a range attack.</returns
        private float3 MagicTargetPosition(ref MapVision mapVision, ref AttackState state)
        {   
            if (!state.CapableOfMagic) return float3.zero;
            foreach(var (staticObject, objectTransform, physicsInfo) in SystemAPI.Query<RefRO<StaticInfluenceObject>, LocalTransform, PhysicsInfo >())
            {
                var distFromObjectToTarget = Vector3.Distance(state.TargetPosition, objectTransform.Position);
                if (distFromObjectToTarget is <= 15 or >= 50) continue;
                var raycastInput = new RaycastInput()
                {
                    Start = objectTransform.Position + new float3(0, 1, 0) + objectTransform.Forward() * 3f,
                    End = state.TargetPosition + new float3(0, 1, 0) ,
                    Filter = new CollisionFilter()
                    {
                        BelongsTo = ((1 << 10)),
                        CollidesWith = physicsInfo.CollidesWith.Value,
                        GroupIndex = 0
                    }
                };
                if (!collisionWorld.CastRay(raycastInput, out RaycastHit raycastHit)) continue;
                // add more filtering stuff
                return objectTransform.Position;
            }
            return new float3();
        }

        private float3 MeleeTargetPosition(MapVision map, AttackState state)
        {
            if (!state.CapableOfMelee) return float3.zero;
            return GlobalFunctions.RandomPoint(state.TargetPosition, 3.5f, out float3 target) ? target : float3.zero;
        }

        /// <summary>
        /// Determines the target position for a range attack by looking for cover position with line of sight
        /// </summary>
        /// <param name="mapVision">The MapVision component containing possible position for NPC to go to .</param>
        /// <param name="state">The AttackState component representing the attack state.</param>
        /// <returns>The target position for a range attack.</returns
        private float3 RangeTargetPosition(ref MapVision mapVision, ref AttackState state)
        {   
            if (!state.CapableOfProjectile) return float3.zero;
            foreach(var (staticObject, objectTransform, physicsInfo) in SystemAPI.Query<RefRO<StaticInfluenceObject>, LocalTransform, PhysicsInfo >())
            {
                var distFromObjectToTarget = Vector3.Distance(state.TargetPosition, objectTransform.Position);
                if (distFromObjectToTarget is <= 15 or >= 50) continue;
                var raycastInput = new RaycastInput()
                {
                    Start = objectTransform.Position + new float3(0, 1, 0) + objectTransform.Forward() * 3f,
                    End = state.TargetPosition + new float3(0, 1, 0) ,
                    Filter = new CollisionFilter()
                    {
                        BelongsTo = ((1 << 10)),
                        CollidesWith = physicsInfo.CollidesWith.Value,
                        GroupIndex = 0
                    }
                };
                if (!collisionWorld.CastRay(raycastInput, out RaycastHit raycastHit)) continue;
                // add more filtering stuff
                return objectTransform.Position;
            }

            return float3.zero;
        }

        public float3 ClosestSafeLocation( MapVision mapVision, EvadeThreat state, LocalTransform transform)
        {
            var lowestDist = float.MaxValue;
            var outputPosition = new float3();
            foreach (var (staticObject, objectTransform, physicsInfo) in SystemAPI
                         .Query<RefRO<StaticInfluenceObject>, LocalTransform, PhysicsInfo>())
            {
                var distFromObjectToTarget = Vector3.Distance(transform.Position, objectTransform.Position);
                if (!(distFromObjectToTarget > lowestDist)) continue;
                lowestDist = distFromObjectToTarget;
                outputPosition = objectTransform.Position;
            }

            return outputPosition;
        }
        public float3 ClosestSafeLocationInfluenceBased( MapVision mapVision, EvadeThreat state, LocalTransform transform)
        {
            var lowestDist = float.MaxValue;
            var outputPosition = new float3();
            foreach (var (staticObject, objectTransform, physicsInfo) in SystemAPI
                         .Query<RefRO<StaticInfluenceObject>, LocalTransform, PhysicsInfo>())
            {
                var distFromObjectToTarget = Vector3.Distance(transform.Position, objectTransform.Position);
                if (!(distFromObjectToTarget > lowestDist)) continue;
                lowestDist = distFromObjectToTarget;
                outputPosition = objectTransform.Position;
            }

            return outputPosition;
        }
        public float3 FarthestSafeLocation(ref MapVision mapVision, ref EvadeThreat state, LocalTransform transform)
        {
            var dist = float.MinValue;
            var outputPosition = new float3();
            foreach (var (staticObject, objectTransform, physicsInfo) in SystemAPI
                         .Query<RefRO<StaticInfluenceObject>, LocalTransform, PhysicsInfo>())
            {
                var distFromObjectToTarget = Vector3.Distance(transform.Position, objectTransform.Position);
                if (!(distFromObjectToTarget < dist) && dist > 75.0f) continue; // Todo 75.0f make  variable based on character stats 
                dist = distFromObjectToTarget;
                outputPosition = objectTransform.Position;
            }

            return outputPosition;
        }
    }
}