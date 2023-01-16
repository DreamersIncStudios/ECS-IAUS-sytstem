using MotionSystem.Components;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Stats.Entities;
namespace DreamersInc.Global
{
    public partial class InputSystem : SystemBase
    {
        Transform m_mainCam;
        protected override void OnCreate()
        {
            base.OnCreate();
         
        }

        protected override void OnUpdate()
        {
            if (m_mainCam == null)
            {
                if (Camera.main != null)
                {
                    m_mainCam = Camera.main.transform;
                }
                else
                {
                    Debug.LogWarning(
        "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
                    // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
                }
            }
            if (!SystemAPI.TryGetSingleton<ControllerInfo>(out var config))
                return;
    


            Entities.WithoutBurst().ForEach((ref CharControllerE Control, in Player_Control PC) =>
            {

                bool m_Crouching = new bool();
                if (!Control.Casting)
                {
                    if (Control.block)
                    {
                        Control.H = 0.0f;
                        Control.V = 0.0f;
                    }
                    else
                    {
                        if (Mathf.Abs(Input.GetAxis("Horizontal")) > .1f)
                            Control.H = Input.GetAxis("Horizontal");
                        else
                            Control.H = 0.0f;
                        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f)
                            Control.V = Input.GetAxis("Vertical");
                        else
                            Control.V = 0.0f;

                        m_Crouching = Input.GetKey(KeyCode.C);

                        if (!PC.InSafeZone)
                        {
                            if (!Control.Jump && Control.IsGrounded)
                            {
                                Control.Jump = config.Jumpb;

                            }

                            // add controller toogle
                            Control.Walk = Input.GetKey(KeyCode.LeftShift);

                        }
                        else
                        {
                            Control.Walk = true;
                        }

                    }
                }


            }).Run();

            Vector3 m_CamForward = new Vector3();

            Entities.WithoutBurst().ForEach((AnimatorComponent Anim, ref CharControllerE Control) =>
            {
                if (!Control.AI)
                {
                    if (m_mainCam != null)
                    {
                        m_CamForward = Vector3.Scale(m_mainCam.forward, new Vector3(1, 0, 1)).normalized;
                        Control.Move = Control.V * m_CamForward + Control.H * m_mainCam.right;
                    }
                    else
                    {
                        Control.Move = Control.V * Vector3.forward + Control.H * Vector3.right;
                    }
                }
                if (Control.Walk)
                    Control.Move *= 0.5f;
                if (Control.Move.magnitude > 1.0f)
                    Control.Move.Normalize();
                Control.Move = Anim.transform.InverseTransformDirection(Control.Move);

                // This section of code can be moved to a  job??


            }).Run();

        }
    }

}
