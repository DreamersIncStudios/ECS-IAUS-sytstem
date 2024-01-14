using MotionSystem.Components;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace MotionSystem.Systems
{
    public class AnimationSpeed : MonoBehaviour
    {
        public bool IsGrounded { get; set; }
        Animator m_Animator;
        Rigidbody m_Rigidbody;
        public float m_MoveSpeedMultiplier { get; set; }

        public void Start()
        {
            m_Animator = GetComponent<Animator>();
            m_Rigidbody = GetComponent<Rigidbody>();
        }
        public void OnAnimatorMove()
        {
            // we implement this function to override the default root motion.
            // this allows us to modify the positional speed before it's applied.
            if (IsGrounded && Time.deltaTime > 0)
            {
                Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

                // we preserve the existing y part of the current velocity.
                v.y = m_Rigidbody.velocity.y;
                m_Rigidbody.velocity = v;
            }
        }

    }
    public class AnimationSpeedLink : IComponentData {
        
        public AnimationSpeed link;
    }

    public partial class AnimationSync : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((AnimationSpeedLink Anim, ref CharControllerE control) => {
               Anim.link.IsGrounded = control.IsGrounded;
                Anim.link.m_MoveSpeedMultiplier = control.m_MoveSpeedMultiplier;
            
            }).WithoutBurst().Run();
        }
    }
}