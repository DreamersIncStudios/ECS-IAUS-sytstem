using UnityEngine;
using DG.Tweening;
using Unity.Entities;
using MotionSystem.Components;
using DreamersIncStudio.CameraControlSystem;
using static PrimeTween.Tween;
// ReSharper disable Unity.BurstLoadingManagedType

namespace MotionSystem.Systems
{

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class AnimatorUpdate : SystemBase
    {
        private static readonly int Forward = Animator.StringToHash("Forward");
        private static readonly int Turn = Animator.StringToHash("Turn");
        private static readonly int Crouch = Animator.StringToHash("Crouch");
        private static readonly int OnGround = Animator.StringToHash("OnGround");
        private static readonly int Property = Animator.StringToHash("Weapon Drawn");
        private static readonly int IsTargeting = Animator.StringToHash("IsTargeting");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int JumpLeg = Animator.StringToHash("JumpLeg");
        private static readonly int Property1 = Animator.StringToHash("Weapon In Hand");
        const float k_Half = 0.5f;


        protected override void OnUpdate()
        {



            Entities.WithoutBurst().ForEach((Animator anim, Rigidbody rb, ref CharControllerE control) =>
            {

        
                Transform transform = anim.transform;
                //if (Anim.GetFloat("AnimSpeed") != control.AnimationSpeed)
                //    Anim.SetFloat("AnimSpeed", control.AnimationSpeed);

               


                //control.Move = Vector3.ProjectOnPlane(control.Move, control.GroundNormal);

                //  m_TurnAmount = control.Move.x;
               var  mForwardAmount = control.Move.z;
                var m_TurnAmount = Mathf.Atan2(control.Move.x, control.Move.z);

                if (!control.Targetting)
                {
                    float turnSpeed = Mathf.Lerp(control.m_StationaryTurnSpeed, control.m_MovingTurnSpeed, mForwardAmount);
                    transform.Rotate(0, m_TurnAmount * turnSpeed * SystemAPI.Time.fixedDeltaTime, 0);
                }
                else
                {

                    m_TurnAmount = control.Move.x;
                    if (!control.AI)
                    {
                        if (CameraControl.Instance.Target.LookAt != null)
                        {

                            var forwardDirection = CameraControl.Instance.Target.LookAt.transform.position -
                                                   transform.position;
                            var rot = Quaternion.LookRotation(forwardDirection);
                            if (transform.rotation != rot)
                                Rotation(transform, rot, 0.5f);

                        }
                    }
                }



                if (control.IsGrounded)
                {
                    HandleGroundedMovement(control, anim, rb);
                }
                else
                {
                    HandleAirborneMovement(control, anim, rb);
                }

                if (control.ApplyRootMotion)
                {
                    anim.applyRootMotion = true;
                    control.ApplyRootMotion = false;
                }

                //ScaleCapsules Collider

                //AutoCrouch 


                // Animator Updater
                // update the animator parameters
                anim.SetFloat(Forward, mForwardAmount, 0.1f, SystemAPI.Time.fixedDeltaTime);
                anim.SetFloat(Turn, m_TurnAmount, 0.1f, SystemAPI.Time.fixedDeltaTime);
                anim.SetBool(Crouch, control.Crouch);
                anim.SetBool(OnGround, control.IsGrounded);
                if (control.CombatCapable)
                {
                    anim.SetBool(Property, control.EquipWeapon);
                    anim.SetBool(IsTargeting, control.Targetting);
                }
                if (!control.IsGrounded)
                {
                    anim.SetFloat(Jump, rb.linearVelocity.y);
                }

                // calculate which leg is behind, to leave that leg trailing in the jump animation
                // (This code is reliant on the specific run cycle offset in our animations,
                // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
                float runCycle =
                    Mathf.Repeat(
                        anim.GetCurrentAnimatorStateInfo(0).normalizedTime + control.m_RunCycleLegOffset, 1);
                float jumpLeg = (runCycle < k_Half ? 1 : -1) * mForwardAmount;
                if (control.IsGrounded)
                {
                    anim.SetFloat(JumpLeg, jumpLeg);
                }

                // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
                // which affects the movement speed because of the root motion.
                if (control.IsGrounded && control.Move.magnitude > 0)
                {
                    anim.speed = control.m_AnimSpeedMultiplier;
                }
                else
                {
                    // don't use that while airborne
                    anim.speed = 1;
                }

                control.Jump = false;



                control.TimerForEquipReset = anim.GetBool(Property1) && control.TimerForEquipReset <= 0.0f && !anim.GetCurrentAnimatorStateInfo(0).IsName("Locomation_Grounded_Weapon0")
                    ? control.EquipResetTimer : anim.GetCurrentAnimatorStateInfo(0).IsTag("Combo") ? control.EquipResetTimer : control.TimerForEquipReset;

                if (control.TimerForEquipReset > 0.0f && anim.GetCurrentAnimatorStateInfo(0).IsName("Locomation_Grounded_Weapon0"))
                {
                    control.TimerForEquipReset -= 0.02f;
                    if (control.TimerForEquipReset < 0.0f)
                    {
                        control.TimerForEquipReset = 0.0f;
                        anim.SetBool("Weapon In Hand", false);
                    }
                }



            }).Run();

            Entities.WithoutBurst().WithChangeFilter<CharControllerE>().ForEach((CapsuleCollider capsule, ref CharControllerE Control) =>
            {

                capsule.center = Control.CapsuleCenter;
                capsule.height = Control.CapsuleHeight;

            }).Run();

            UpdateBeast();

        }
        void HandleGroundedMovement(CharControllerE control, Animator Anim, Rigidbody RB)
        {
            if (control.Jump && !control.Crouch)
            {
                if (Anim.GetCurrentAnimatorStateInfo(0).IsName("Grounded0")
                || Anim.GetCurrentAnimatorStateInfo(0).IsName("Locomation_Grounded_Weapon0")
                || Anim.GetCurrentAnimatorStateInfo(0).IsName("Targeted_Locomation0"))
                {
                    // jump!
                    Anim.applyRootMotion = false;
                    RB.linearVelocity = new Vector3(RB.linearVelocity.x, control.m_JumpPower, RB.linearVelocity.z);
                    control.IsGrounded = false;
                    control.GroundCheckDistance = 0.1f;
                    control.SkipGroundCheck = true;
                }
            }
        }
        void HandleAirborneMovement(CharControllerE control, Animator Anim, Rigidbody RB)
        {
            Vector3 extraGravityForce = (Physics.gravity * control.m_GravityMultiplier) - Physics.gravity;
            RB.AddForce(extraGravityForce);

            control.SkipGroundCheck = RB.linearVelocity.y > 0;
            control.GroundCheckDistance = RB.linearVelocity.y < 0 ? control.m_OrigGroundCheckDistance : 0.1f;

            Anim.applyRootMotion =
            control.ApplyRootMotion;

        }

        void UpdateBeast()
        {

            Entities.WithoutBurst().ForEach((Animator Anim, Transform transform, Rigidbody RB, ref BeastControllerComponent control) =>
            {
                if (Anim.GetFloat("AnimSpeed") != control.AnimationSpeed)
                    Anim.SetFloat("AnimSpeed", control.AnimationSpeed);

                float m_TurnAmount;
                float m_ForwardAmount;

                m_ForwardAmount = control.Move.z;
                m_TurnAmount = Mathf.Atan2(control.Move.x, control.Move.z);

                if (!control.Targetting)
                {
                    float turnSpeed = Mathf.Lerp(control.m_StationaryTurnSpeed, control.m_MovingTurnSpeed, m_ForwardAmount);
                    transform.Rotate(0, m_TurnAmount * turnSpeed * SystemAPI.Time.fixedDeltaTime, 0);
                }
                else
                {

                    m_TurnAmount = control.Move.x;
                    if (!control.AI)
                    {
                        if (CameraControl.Instance.Target.LookAt != null)
                        {

                            var forwardDirection = CameraControl.Instance.Target.LookAt.transform.position -
                                                   transform.position;
                            var rot = Quaternion.LookRotation(forwardDirection);
                            if (transform.rotation != rot)
                                Rotation(transform, rot, 0.5f);

                        }
                    }
                }

                if (control.IsGrounded)
                {
                    HandleGroundedMovement(control, Anim, RB);
                }
                else
                {
                    HandleAirborneMovement(control, Anim, RB);
                }

                if (control.ApplyRootMotion)
                {
                    Anim.applyRootMotion = true;
                    control.ApplyRootMotion = false;
                }

                // Animator Updater
                // update the animator parameters
                Anim.SetFloat("Forward", m_ForwardAmount, 0.1f, SystemAPI.Time.fixedDeltaTime);
                Anim.SetFloat("Turn", m_TurnAmount, 0.1f, SystemAPI.Time.fixedDeltaTime);
                Anim.SetBool("OnGround", control.IsGrounded);

                // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
                // which affects the movement speed because of the root motion.
                if (control.IsGrounded && control.Move.magnitude > 0)
                {
                    Anim.speed = control.m_AnimSpeedMultiplier;
                }
                else
                {
                    // don't use that while airborne
                    Anim.speed = 1;
                }

                control.Jump = false;




            }).Run();

            Entities.WithoutBurst().WithChangeFilter<BeastControllerComponent>().ForEach((CapsuleCollider capsule, ref BeastControllerComponent Control) =>
            {

                capsule.center = Control.CapsuleCenter;
                capsule.height = Control.CapsuleHeight;

            }).Run();
        }

        void HandleGroundedMovement(BeastControllerComponent control, Animator Anim, Rigidbody RB)
        {
            if (control.Jump)
            {
                if (Anim.GetCurrentAnimatorStateInfo(0).IsName("Grounded0")
                || Anim.GetCurrentAnimatorStateInfo(0).IsName("Locomation_Grounded_Weapon0")
                || Anim.GetCurrentAnimatorStateInfo(0).IsName("Targeted_Locomation0"))
                {
                    // jump!
                    Anim.applyRootMotion = false;
                    RB.linearVelocity = new Vector3(RB.linearVelocity.x, control.m_JumpPower, RB.linearVelocity.z);
                    control.IsGrounded = false;
                    control.GroundCheckDistance = 0.1f;
                    control.SkipGroundCheck = true;
                }
            }
        }
        void HandleAirborneMovement(BeastControllerComponent control, Animator Anim, Rigidbody RB)
        {
            Vector3 extraGravityForce = (Physics.gravity * control.m_GravityMultiplier) - Physics.gravity;
            RB.AddForce(extraGravityForce);

            control.SkipGroundCheck = RB.linearVelocity.y > 0;
            control.GroundCheckDistance = RB.linearVelocity.y < 0 ? control.m_OrigGroundCheckDistance : 0.1f;

            Anim.applyRootMotion = control.ApplyRootMotion;

        }
    }



}