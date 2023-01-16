using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace MotionSystem.Components
{
    public struct CharControllerE : IComponentData
    {

        public float3 CapsuleCenter;
        public float CapsuleRadius;
        public float CapsuleHeight;
        public float3 OGCapsuleCenter { get; set; }
        public float OGCapsuleHeight { get; set; }
        public float H;
        public float V;
        public bool Jump { get; set; }
        public bool Crouch;

        public bool CombatCapable;
        public bool ApplyRootMotion;
        [SerializeField]public bool SkipGroundCheck { get; set; }
        public Vector3 Move;
        public bool Walk;
        public Vector3 GroundNormal;
        public bool IsGrounded;
        public float GroundCheckDistance;
        public bool block;
        public float m_MovingTurnSpeed;
        public float m_StationaryTurnSpeed;
        public float m_JumpPower;
        public float m_GravityMultiplier;
        public float m_RunCycleLegOffset; //specific to the character in sample assets, will need to be modified to work with others
        public float m_MoveSpeedMultiplier;
        public float m_AnimSpeedMultiplier;
        public float m_OrigGroundCheckDistance;
        public bool AI;

        public bool EquipWeapon => TimerForEquipReset > 0.0f;
        public float EquipResetTimer;
        public float TimerForEquipReset { get; set; }
        public float AnimationSpeed { get; set; }
        public bool Targetting;
        public bool Casting;
        //Todo Add back
        // => AnimationSpeed < 1.0f;
    }

    public struct AI_Control : IComponentData
    {
        public bool IsGrounded;
    }

}

