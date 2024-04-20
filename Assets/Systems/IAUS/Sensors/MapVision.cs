using DreamersInc.InflunceMapSystem;
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
        public bool HasMeleeLocation => !Locations.c0.Equals(float3.zero);
        public bool HasRangeLocation => !Locations.c2.Equals(float3.zero);
        public bool HasMagicLocation => !Locations.c1.Equals(float3.zero);

        public float3x3 Locations;
    }

    public partial class MapVisionSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<PhysicsWorldSingleton>();
            EntityManager.CompleteDependencyBeforeRO<PhysicsWorldSingleton>();
        }

        protected override void OnUpdate()
        {

            collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            Entities.WithAll<AttackActionTag>().ForEach((ref MapVision mapVision, ref AttackState state) =>
                {
                    mapVision.Locations.c0 = MeleeTargetPosition( state);
                    mapVision.Locations.c1 = MagicTargetPosition(ref mapVision, ref state);
                    mapVision.Locations.c2 = RangeTargetPosition(ref mapVision, ref state);
                }).WithoutBurst()
                .Run();
        }

        private CollisionWorld collisionWorld;
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

        private float3 MeleeTargetPosition( AttackState state)
        {
            if (!state.CapableOfMelee) return float3.zero;
            return GlobalFunctions.RandomPoint(state.TargetPosition, 7.5f, out float3 target) ? target : float3.zero;
        }

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
            return new float3();
        }
    }
}