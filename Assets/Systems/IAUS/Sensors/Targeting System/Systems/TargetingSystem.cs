using AISenses.VisionSystems.Combat;
using UnityEngine;
using Unity.Entities;
using DreamersIncStudio.CameraControlSystem;
using Unity.Collections;
using DreamersInc;
using DreamersInc.InputSystems;
using MotionSystem.Components;
using UnityEngine.InputSystem;

namespace AISenses.VisionSystems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class TargetingSystem : SystemBase
    {
        private PlayerControls playerControls;

        protected override void OnCreate()
        {
            RequireForUpdate<Player_Control>();
            RequireForUpdate<InputSingleton>();
            if (SystemAPI.ManagedAPI.TryGetSingleton<InputSingleton>(out var inputSingle))
            {
                playerControls = inputSingle.ControllerInput;
            }
        }

        private CameraControl cameraControl;
        protected override void OnStartRunning()
        {
            if (playerControls == null)
            {
                if (SystemAPI.ManagedAPI.TryGetSingleton<InputSingleton>(out var inputSingle))
                {
                    playerControls = inputSingle.ControllerInput;
                }
            }
            cameraControl = CameraControl.Instance;
            if (playerControls == null) return;
            playerControls.PlayerController.LockOn.performed += ToggleTargeting;
            playerControls.PlayerController.ChangeTargetNeg.performed += ChangeTargetNegative;
            playerControls.PlayerController.ChangeTargetPos.performed += ChangeTargetPositive;
        }

        protected override void OnStopRunning()
        {
            playerControls.PlayerController.LockOn.performed -= ToggleTargeting;
            playerControls.PlayerController.ChangeTargetNeg.performed -= ChangeTargetNegative;
            playerControls.PlayerController.ChangeTargetPos.performed -= ChangeTargetPositive;
        }

        protected override void OnUpdate()
        {
        
        }
        int index = 0;
        void ToggleTargeting(InputAction.CallbackContext obj)
        {
            if(!cameraControl)
                cameraControl = CameraControl.Instance;
            Entities.WithoutBurst().WithAll<Player_Control>().ForEach((DynamicBuffer<Enemies> buffer, ref CharControllerE control,ref AttackTarget attackTarget) =>
            {
                var sortedBuffer = buffer.AsNativeArray();
                sortedBuffer.Sort( new SortScanPositionByDistance());
                
                control.Targetting = !control.Targetting;
                cameraControl.OnTargetingChanged?.Invoke(this,
                    new CameraControl.OnTargetingChangedEventArgs() { isTargeting = control.Targetting });

                index = 0;
                SetTarget(sortedBuffer);
                attackTarget.AttackTargetIndex = control.Targetting ? index : -1;
                attackTarget.IsTargeting = control.Targetting;
                
            }).Run();
        }

        void ChangeTargetPositive(InputAction.CallbackContext obj)
        {
            if(!cameraControl)
                cameraControl = CameraControl.Instance;
            Entities.WithoutBurst().WithAll<Player_Control>().ForEach((DynamicBuffer<Enemies> buffer, ref CharControllerE control, ref AttackTarget attackTarget) =>
            {     
                if(!control.Targetting) return; 
                var sortedBuffer = buffer.AsNativeArray();
                sortedBuffer.Sort( new SortScanPositionByDistance());
                index++;
                if (index > buffer.Length - 1)
                    index = 0;
                SetTarget(sortedBuffer);
                attackTarget.AttackTargetIndex = control.Targetting ? index : -1;
            }).Run();
        }
        void ChangeTargetNegative(InputAction.CallbackContext obj)
        {
            if(!cameraControl)
                cameraControl = CameraControl.Instance;
            Entities.WithoutBurst().WithAll<Player_Control>().ForEach((DynamicBuffer<Enemies> buffer, ref CharControllerE control,  ref AttackTarget attackTarget) =>
            {      
                if(!control.Targetting) return; 
                var sortedBuffer = buffer.AsNativeArray();
                sortedBuffer.Sort( new SortScanPositionByDistance());
                index--;
                if (index < 0)
                    index = buffer.Length - 1;
                
                attackTarget.AttackTargetIndex = control.Targetting ? index : -1;
                SetTarget(sortedBuffer);
            }).Run();
        }

        private void SetTarget(NativeArray<Enemies> sortedBuffer)
        {
            if (cameraControl.OnTargetChanged != null)
            {
                if (sortedBuffer.Length == 0)
                {
                    cameraControl.OnTargetChanged(this,
                        new CameraControl.OnTargetChangedEventArgs(null));
                }
                else
                  cameraControl.OnTargetChanged(this,
                    new CameraControl.OnTargetChangedEventArgs(EntityManager
                        .GetComponentObject<Animator>(sortedBuffer[index].target.Entity)
                        .gameObject));
            }
        }
    }
}