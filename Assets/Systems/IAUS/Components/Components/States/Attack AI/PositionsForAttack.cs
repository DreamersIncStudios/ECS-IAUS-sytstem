using DreamersInc.InfluenceMapSystem;
using Global.Component;
using IAUS.ECS.Systems;
using IAUS.ECS.Systems.Reactive;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace IAUS.ECS.Component.Attacking
{
    internal interface AttackPosition : IBufferElementData
    {
    
        public OccupiedState State { get; set; }
        public float Usability { get; set; }
        void SetPosition(float3 position);
    }

    public enum OccupiedState
    {
        Vacant, Reserved, Occupied
    }

    [InternalBufferCapacity(6)]
    public struct MeleeAttackPosition : AttackPosition
    {
        public float3 Position;
        [SerializeField]  public OccupiedState State { get; set; }
        [SerializeField]  public float Usability { get; set; }

        public static implicit operator float3(MeleeAttackPosition e)
        {
            return e.Position;
        }

        public static implicit operator MeleeAttackPosition(float3 e)
        {
            return new MeleeAttackPosition { Position = e };
        }


        public void SetPosition(float3 position)
        {
            Position = position;
        }
    }
    
    [InternalBufferCapacity(10)]
    public struct RangeAttackPosition : AttackPosition
    {
        [SerializeField]public float3 Position { get; set; }
        [SerializeField] public OccupiedState State { get; set; }
        [SerializeField]  public float Usability { get; set; }
        
        public void SetPosition(float3 position)
        {
            Position = position;
        }
        
    }

    public struct ReserveLocationTag : IBufferElementData
    {
        public int ID;
        public Entity ReserverEntity;

    }

    [UpdateInGroup(typeof(IAUSUpdateGroup))]
    [UpdateAfter(typeof(AttackTagReactor.AttackUpdateSystem))]
    public partial class UpdateAttackPositionSystem : SystemBase
    {
        private CollisionWorld collisionWorld;

        protected override void OnUpdate()
        {

            Entities.WithChangeFilter<LocalToWorld>().WithoutBurst().ForEach(
                (ref LocalToWorld transform, ref DynamicBuffer<RangeAttackPosition> attackPosition) =>
                {
                    for (var i = 0; i < 6; i++)
                    {
                        var temp = attackPosition[i];
                        if ((temp.State == OccupiedState.Vacant &&
                             Vector3.Distance(temp.Position, transform.Position) > 25.5f) ||
                            Vector3.Distance(temp.Position, transform.Position) > 50.5f)
                        {
            
                            var position = DetermineRangedAttackPositionValidity(transform);
                            temp.SetPosition(position);
            
                        }
            
                        attackPosition[i] = temp;
                    }
                }).Run();
            
            Entities.WithChangeFilter<LocalToWorld>().ForEach(
                    ( DynamicBuffer<MeleeAttackPosition> attackPosition, ref LocalToWorld transform) =>
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            var temp = attackPosition[i];
                            if (temp.State == OccupiedState.Vacant &&
                                Vector3.Distance(temp.Position, transform.Position) > 10.5f)
                            {
                                var target = i switch
                                {
                                    0 => transform.Position + transform.Forward * 2,
                                    1 => transform.Position + transform.Right * 2,
                                    2 => transform.Position - transform.Forward * 2,
                                    3 => transform.Position - transform.Right * 2,
                                    _ => new float3()
                                };
                                temp.SetPosition(target);
                            }
            
                            attackPosition[i] = temp;
                        }
                    })
                .ScheduleParallel();

            ComponentLookup<AttackState> lookup = SystemAPI.GetComponentLookup<AttackState>(false);
            Entities.ForEach((Entity entity,DynamicBuffer<ReserveLocationTag> tags,
                 DynamicBuffer<MeleeAttackPosition> attackPosition) =>
            {
                for (var index = 0; index < tags.Length; index++)
                {
                    var tag = tags[index];
                    if (attackPosition[tag.ID].State != OccupiedState.Vacant)
                    {
                        tags.RemoveAt(index);
                    }
                    else
                    {
                        var temp = attackPosition[tag.ID];
                        temp.State = OccupiedState.Occupied;
                        var state = lookup[tag.ReserverEntity];
                        attackPosition[tag.ID] = temp;
                        state.TargetPosition = temp;
                        state.AttackPlans.RemoveAt(0);
                        tags.RemoveAt(index);
                        state.TargetPositionID = index;
                        lookup[tag.ReserverEntity] = state;
                    }
                }
            }).Schedule(); 

        }

  
        private float3 DetermineRangedAttackPositionValidity(LocalToWorld transform)
        {
            foreach (var (staticObject, objectTransform, physicsInfo) in SystemAPI
                         .Query<RefRO<StaticInfluenceObject>, LocalTransform, PhysicsInfo>())
            {
                var distFromObjectToTarget = Vector3.Distance(transform.Position, objectTransform.Position);
                if (distFromObjectToTarget is <= 15 or >= 50) continue;
                var raycastInput = new RaycastInput()
                {
                    Start = objectTransform.Position + new float3(0, 1, 0) + objectTransform.Forward() * 3f,
                    End = transform.Position + new float3(0, 1, 0),
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
    }
}