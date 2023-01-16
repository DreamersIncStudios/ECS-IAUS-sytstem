using MotionSystem.Components;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using DreamersInc;

namespace MotionSystem
{
 

    public class PlayerCharacterControllerAuthoring : MonoBehaviour
    {
        public CapsuleCollider Capsule;
        public bool AI_Control;
        public bool Party;
        public bool IsPlayer;
        public bool CombatCapable;
        [Header("Weapon Specs")]
        public float EquipResetTimer = 5.0f;
        [Header("Animation Movement Specs")]
        [SerializeField] float m_MovingTurnSpeed = 360;
        [SerializeField] float m_StationaryTurnSpeed = 180;
        [SerializeField] float m_JumpPower = 12f;
        [Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
        [SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
        [SerializeField] float m_MoveSpeedMultiplier = 1f;
        [SerializeField] float m_AnimSpeedMultiplier = 1f;
        [SerializeField] float m_GroundCheckDistance = 0.1f;
        [SerializeField] float3 GroundProbeVector;

        class Player : Baker<PlayerCharacterControllerAuthoring>
        {
            public override void Bake(PlayerCharacterControllerAuthoring authoring)
            {
               CharControllerE data= new CharControllerE()
               {
                   CapsuleRadius = authoring.Capsule.radius,
                   OGCapsuleHeight = authoring.Capsule.height,
                   OGCapsuleCenter = authoring.Capsule.center,
                   CapsuleCenter = authoring.Capsule.center,
                   CapsuleHeight = authoring.Capsule.height,
                   IsGrounded = true,
                   AI = authoring.AI_Control,
                   CombatCapable = authoring.CombatCapable,
                   EquipResetTimer = authoring.EquipResetTimer,
                   m_AnimSpeedMultiplier = authoring.m_AnimSpeedMultiplier,
                   m_GravityMultiplier = authoring.m_GravityMultiplier,
                   m_JumpPower = authoring.m_JumpPower,
                   m_MoveSpeedMultiplier = authoring.m_MoveSpeedMultiplier,
                   m_MovingTurnSpeed = authoring.m_MovingTurnSpeed,
                   m_RunCycleLegOffset = authoring.m_RunCycleLegOffset,
                   m_StationaryTurnSpeed = authoring.m_StationaryTurnSpeed,
                   m_OrigGroundCheckDistance = authoring.m_GroundCheckDistance,
                   GroundCheckDistance = authoring.m_GroundCheckDistance
               }
                    
                    ; 

                AddComponent(data);
                AddComponentObject(new TransformGO() { transform = authoring.Capsule.transform });
                if(authoring.IsPlayer)
                    AddComponent(new Player_Control() { });
            }
        }
    }
}
