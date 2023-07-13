using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
namespace MotionSystem.Components
{
    public struct BeastControllerComponent : IComponentData
    {
        public float3 CapsuleCenter;
        public float CapsuleRadius;
        public float CapsuleHeight;
        public float3 OGCapsuleCenter { get; set; }
        public float OGCapsuleHeight { get; set; }
        public float H { get; set; }
        public float V { get; set; }
        public bool Jump { get; set; }
        public bool ApplyRootMotion;
        [SerializeField] public bool SkipGroundCheck { get; set; }
        public Vector3 Move { get; set; }
        public bool Walk;
        public Vector3 GroundNormal;
        public bool IsGrounded;
        public float GroundCheckDistance;
        public float m_MovingTurnSpeed;
        public float m_StationaryTurnSpeed;
        public float m_JumpPower;
        public float m_GravityMultiplier;
        public float m_MoveSpeedMultiplier;
        public float m_AnimSpeedMultiplier;
        public float m_OrigGroundCheckDistance;
        public bool AI;
        public float AnimationSpeed { get; set; }
        public bool Targetting;
        public bool Casting => AnimationSpeed < 1.0f;
    }
}
