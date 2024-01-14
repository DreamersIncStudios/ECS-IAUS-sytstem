
using System;
using DreamersInc.ComboSystem;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
// ReSharper disable Unity.BurstLoadingManagedType

namespace DreamersInc.InputSystems
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial class ButtonInputSystem : SystemBase
    {
        private PlayerControls playerControls;
        private void CreateInput()
        {
            playerControls = new PlayerControls();
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var data = new InputSingleton()
            {
                ControllerInput= playerControls
            };

            Entity gm = manager.CreateSingleton(data);
#if UNITY_EDITOR
            manager.SetName(gm, "Input");
#endif
             
        }
        protected override void OnCreate()
        {
            RequireForUpdate<Player_Control>();
            CreateInput();
        }

        protected override void OnStartRunning()
        {
            block = false;
            playerControls.Enable();
            playerControls.PlayerController.LightAttack.performed += OnPlayerLightAttack;
            playerControls.PlayerController.HeavyAttack.performed += OnPlayerHeavyAttack;
            playerControls.PlayerController.Projectile.performed += OnPlayerShoot;
            playerControls.PlayerController.Dodge.performed += OnPlayerDodge;
            playerControls.PlayerController.Block.performed += OnPlayerBlock;
            playerControls.PlayerController.OpenCadMenu.performed += ToggleMagicInput;
            playerControls.MagicController.OpenCadMenu.performed += ToggleMagicInput;
            playerControls.MagicController.LightAttack.performed += OnXMagicAttack;
            playerControls.MagicController.HeavyAttack.performed += OnYMagicAttack;
            playerControls.MagicController.Jump.performed += OnAMagicAttack;
            playerControls.MagicController.Dodge.performed += OnBMagicAttack;
           
        }

        protected override void OnStopRunning()
        {
            playerControls.Disable();
            playerControls.SafeAreaPlayerController.Disable();
            playerControls.PlayerController.LightAttack.performed -= OnPlayerLightAttack;
            playerControls.PlayerController.HeavyAttack.performed -= OnPlayerHeavyAttack;
            playerControls.PlayerController.Projectile.performed -= OnPlayerShoot;
            playerControls.PlayerController.Dodge.performed -= OnPlayerDodge;
            playerControls.PlayerController.Block.performed -= OnPlayerBlock;
            playerControls.PlayerController.OpenCadMenu.performed -= ToggleMagicInput;
            playerControls.MagicController.OpenCadMenu.performed -= ToggleMagicInput;
            playerControls.MagicController.LightAttack.performed -= OnXMagicAttack;
            playerControls.MagicController.HeavyAttack.performed -= OnYMagicAttack;
            playerControls.MagicController.Jump.performed -= OnAMagicAttack;
            playerControls.MagicController.Dodge.performed -= OnBMagicAttack;
        
        }

        private void OnPlayerLightAttack(InputAction.CallbackContext obj)
        {

            Entities.WithoutBurst().ForEach(
                (Animator anim, PlayerComboComponent comboList, Command handler, in Player_Control controls) =>
                {
                    if (anim.IsInTransition(0) || comboList.Combo.ShowMovesPanel) return;
                    foreach (ComboSingle comboTest in comboList.Combo.ComboLists)
                    {
                        foreach (var comboOption in comboTest.ComboList)
                        {
                            if (handler.StateInfo.IsName(comboOption.CurrentStateName))
                            {
                                handler.currentStateExitTime = comboOption.AnimationEndTime;
                                if (!comboOption.InputAllowed(handler.StateInfo.normalizedTime)) continue;
                                var trigger = comboOption.Trigger;
                                if (!comboTest.Unlocked || !handler.QueueIsEmpty) continue;
                                if (trigger.attackType == AttackType.LightAttack) handler.InputQueue.Enqueue(trigger);
                            }
                        }
                    }
                }).Run();

        }

        private void OnPlayerHeavyAttack(InputAction.CallbackContext obj)
        {
            Entities.WithoutBurst().ForEach(
                (Animator anim, PlayerComboComponent comboList, Command handler, in Player_Control controls) =>
                {
                    if (anim.IsInTransition(0) || comboList.Combo.ShowMovesPanel) return;
                    foreach (ComboSingle comboTest in comboList.Combo.ComboLists)
                    {
                        foreach (var comboOption in comboTest.ComboList)
                        {
                            if (handler.StateInfo.IsName(comboOption.CurrentStateName))
                            {
                                handler.currentStateExitTime = comboOption.AnimationEndTime;
                                if (!comboOption.InputAllowed(handler.StateInfo.normalizedTime)) continue;
                                var trigger = comboOption.Trigger;
                                if (!comboTest.Unlocked || !handler.QueueIsEmpty) continue;
                                if (trigger.attackType == AttackType.HeavyAttack) handler.InputQueue.Enqueue(trigger);
                            }
                        }
                    }
                }).Run();
        }

        private void OnPlayerShoot(InputAction.CallbackContext obj)
        {
            Entities.WithoutBurst().ForEach(
                (Animator anim, PlayerComboComponent comboList, Command handler, in Player_Control controls) =>
                {
                    if (anim.IsInTransition(0) || comboList.Combo.ShowMovesPanel) return;
                    foreach (ComboSingle comboTest in comboList.Combo.ComboLists)
                    {
                        foreach (var comboOption in comboTest.ComboList)
                        {
                            if (handler.StateInfo.IsName(comboOption.CurrentStateName))
                            {
                                handler.currentStateExitTime = comboOption.AnimationEndTime;
                                if (!comboOption.InputAllowed(handler.StateInfo.normalizedTime)) continue;
                                var trigger = comboOption.Trigger;
                                if (!comboTest.Unlocked || !handler.QueueIsEmpty) continue;
                                if (trigger.attackType == AttackType.Projectile) handler.InputQueue.Enqueue(trigger);
                            }
                        }
                    }
                }).Run();
        }

        private void OnPlayerDodge(InputAction.CallbackContext obj)
        {
            var dir = playerControls.PlayerController.Locomotion.ReadValue<Vector2>();

            Entities.WithoutBurst().ForEach(
                (Animator anim, PlayerComboComponent comboList, Command handler, in Player_Control controls) =>
                {
                    if (anim.IsInTransition(0) || comboList.Combo.ShowMovesPanel) return;
                    if (dir.y > .1)
                    {
                        handler.InputQueue.Enqueue(new AnimationTrigger()
                        {
                            triggerAnimIndex = 0,
                            attackType = AttackType.Dodge,
                            TransitionDuration = .25f,
                        });
                    }
                    else
                    {
                        handler.InputQueue.Enqueue(new AnimationTrigger()
                        {
                            triggerAnimIndex = 1,
                            attackType = AttackType.Dodge,
                            TransitionDuration = 0.25f,

                        });
                    }
                }).Run();
        }

        private bool block;

        private void OnPlayerBlock(InputAction.CallbackContext obj)
        {
            block = !block;
            switchBlock = true;

        }

        private bool casting;
        private static readonly int Block = Animator.StringToHash("Block");

        void ToggleMagicInput(InputAction.CallbackContext obj)
        {
            Debug.Log("I casted light magic");
            if (casting)
            {
                casting = false;
                
                playerControls.PlayerController.Enable();
                Entities.WithoutBurst()
                    .ForEach(( Command handler, ref Player_Control tag) =>
                    {
                        handler.InputTimer = 0.0f;
                    }).Run();
                DisableSlowMoMode();
            }
            else
            {
                casting = true;
                playerControls.PlayerController.Disable();
                playerControls.MagicController.Enable();
                EnableSlowMoMode();
                Entities.WithoutBurst().ForEach(
                    (Command handler, ref Player_Control tag) =>
                    {
                        handler.InputTimer = 5.5f; //Todo make this value based on Stats
                    }).Run();
            }
        }

        void OnXMagicAttack(InputAction.CallbackContext obj)
        {
            Entities.WithoutBurst().ForEach((Command handler,
                ref Player_Control tag) =>
            {
                Debug.Log("x");
                handler.MagicInputQueue.Enqueue("X");
            }).Run();
        }

        void OnYMagicAttack(InputAction.CallbackContext obj)
        {
            Entities.WithoutBurst().ForEach((Command handler,
                ref Player_Control tag) =>
            {
                handler.MagicInputQueue.Enqueue("Y");
            }).Run();
        }

        void OnBMagicAttack(InputAction.CallbackContext obj)
        {
            Entities.WithoutBurst().ForEach((Command handler,
                ref Player_Control tag) =>
            {
                handler.MagicInputQueue.Enqueue("B");
            }).Run();
        }

        void OnAMagicAttack(InputAction.CallbackContext obj)
        {
            Entities.WithoutBurst().ForEach((Command handler,
                ref Player_Control tag) =>
            {
                handler.MagicInputQueue.Enqueue("A");
            }).Run();
        }

        private bool switchBlock;

        protected override void OnUpdate()
        {
            if (block)
            {
                Entities.WithoutBurst().ForEach(
                    (Animator anim, Command handler,
                        in Player_Control controls) =>
                    {
                        anim.SetBool(Block, true);

                        handler.InputQueue.Enqueue(new AnimationTrigger()
                        {
                            attackType = AttackType.Defend,

                        });
                    }).Run();
            }
            else if (switchBlock)
            {
                Entities.WithoutBurst().ForEach(
                    (Animator anim, Command handler,
                        in Player_Control controls) =>
                    {
                        anim.SetBool(Block, false);
                        handler.InputQueue.Clear();
                    }).Run();
                switchBlock = false;

            }

            Entities.WithStructuralChanges().ForEach((Entity entity, in ControllerChangeProfileTag tag) =>
            {
                switch (tag.Profile)
                {
                    case ChangeProfile.Combat:
                        playerControls.PlayerController.Enable();
                        playerControls.SafeAreaPlayerController.Disable();
                        playerControls.UI.Disable();

                        break;
                    case ChangeProfile.SafeZone:
                        playerControls.PlayerController.Disable();
                        playerControls.SafeAreaPlayerController.Enable();
                        playerControls.UI.Disable();

                        break;
                    case ChangeProfile.Menus:
                        break;
                    case ChangeProfile.Magic:
                        break;
                    case ChangeProfile.UI:
                        playerControls.PlayerController.Disable();
                        playerControls.SafeAreaPlayerController.Disable();
                        playerControls.UI.Enable();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<ControllerChangeProfileTag>(entity);

            }).WithoutBurst().Run();
        }


        private void EnableSlowMoMode()
        {
            Entities.WithoutBurst().WithStructuralChanges().WithNone<AnimationSpeedMod>().ForEach(
                (Entity entity, in Animator animC) =>
                {
                    //Todo add range limit;
                    EntityManager.AddComponentData(entity, new AnimationSpeedMod()
                    {
                        SpeedValue = .15f
                    });


                }).Run();

        }

        private void DisableSlowMoMode()
        {
            Entities.WithoutBurst().WithStructuralChanges().WithAll<AnimationSpeedMod>().ForEach(
                (Entity entity, in  Animator animC) => { EntityManager.RemoveComponent<AnimationSpeedMod>(entity); }).Run();

        }


        void OnPlayerToggleWalkSprint(InputAction.CallbackContext obj)
        {
            //Todo add button press change speed 

        }
    }

    public struct ControllerChangeProfileTag : IComponentData
    {
        public ChangeProfile Profile;
    }

    public enum ChangeProfile
    {
        Combat, SafeZone, Menus, Magic, UI
    }
}