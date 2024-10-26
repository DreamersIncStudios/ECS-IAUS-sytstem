using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace IAUS.ECS.Component.Attacking
{
    interface AttackPosition : IBufferElementData
    {
        public float3 Position { get; set; }
        public bool Occupied { get; set; }
        public float Usability { get; set; }
    }

    [InternalBufferCapacity(4)]
    public struct MeleeAttackPosition : AttackPosition
    {
        public float3 Position { get; set; }
        public bool Occupied { get; set; }
        public float Usability { get; set; }
    }
    
    [InternalBufferCapacity(6)]
    public struct RangeAttackPosition : AttackPosition
    {
        public float3 Position { get; set; }
        public bool Occupied { get; set; }
        public float Usability { get; set; }
    }

    public partial class UpdateAttackPositionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }
    }
}