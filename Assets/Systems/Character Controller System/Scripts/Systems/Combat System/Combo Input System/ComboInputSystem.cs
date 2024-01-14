using Unity.Entities;
using System.Collections.Generic;
using Dreamers.InventorySystem;
using DreamersInc.InputSystems;
using MotionSystem.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DreamersInc.ComboSystem
{
    public partial class ComboInputSystem : SystemBase
    {
        private static readonly int WeaponHand = Animator.StringToHash("Weapon In Hand");
        private static readonly int Block = Animator.StringToHash("Block");
        private PlayerControls playerControls;

        protected override void OnCreate()
        {
            RequireForUpdate<Player_Control>();
            RequireForUpdate<InputSingleton>();
            if (SystemAPI.ManagedAPI.TryGetSingleton<InputSingleton>(out var inputSingle))
            {
                playerControls = inputSingle.ControllerInput;
            }


            Entities.WithoutBurst()
                .ForEach((PlayerComboComponent comboList, Animator anim, Command handler, ref Player_Control pc) =>
                {
                    handler.InputQueue = new Queue<AnimationTrigger>();
                }).Run();
        }

        protected override void OnStartRunning()
        {
            if (playerControls == null)
            {
                if (SystemAPI.ManagedAPI.TryGetSingleton<InputSingleton>(out var inputSingle))
                {
                    playerControls = inputSingle.ControllerInput;
                }
            }


            playerControls.MagicController.CloseCadMenu.performed += StartMagicInputHandling;
      

        }

        protected override void OnStopRunning()
        {
            playerControls.Disable();
            playerControls.MagicController.CloseCadMenu.performed -= StartMagicInputHandling;
        }

        //TODO Decouple this code split into small chu
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((Command handler, ref CharControllerE controller, in Player_Control Tag) =>
            {
                //Todo Possible to remove using Action Map to control inputs ???
                if (!Tag.InSafeZone)
                    controller.CastingTimer = handler.CanInputAbilities;
            }).Run();


            Entities.WithoutBurst().WithChangeFilter<CharacterInventory>().ForEach(
                (Command handler, CharacterInventory inventory, ref CharControllerE control) =>
                {
                    if (inventory.Equipment.EquippedWeapons.TryGetValue(
                            Dreamers.InventorySystem.Interfaces.WeaponSlot.Primary, out var weaponSO))
                    {
                        handler.AlwaysDrawnWeapon = weaponSO.AlwaysDrawn;
                        control.EquipOverride = weaponSO.AlwaysDrawn;
                    }
                }).Run();


            Entities.WithoutBurst().ForEach(
                (Entity entity, PlayerComboComponent comboList, Animator Anim, Command handler,
                    ref Player_Control tag) =>
                {

                    handler.InputQueue ??= new Queue<AnimationTrigger>();
                    handler.MagicInputQueue ??= new Queue<string>();
                    if (tag.InSafeZone | !comboList.WeaponEquipped)
                    {
                        // Todo add logic for play to store weapon

                        return;
                    }

                    if (!Anim.GetBool(WeaponHand) && handler.AlwaysDrawnWeapon)
                        Anim.SetBool(WeaponHand, true);


                    if (handler.CanInputAbilities)
                    {
                        handler.InputTimer -= SystemAPI.Time.DeltaTime;
                    }


                }).Run();
            AnimationTriggering();

        }
        private void StartMagicInputHandling(InputAction.CallbackContext obj)
        {
            Debug.Log("Called");
            playerControls.MagicController.Disable();

            Entities.WithoutBurst().ForEach((Entity entity, Command handler, ref Player_Control tag) =>
            {
                if (handler.CanInputAbilities && !handler.HasMagicSpell) return;
                var output = "";
                while (handler.HasMagicSpell)
                {
                    output += handler.MagicInputQueue.Dequeue();
                }

                var skill = handler.EquippedAbilities.GetAbility(output);
                // ReSharper disable once Unity.BurstLoadingManagedType
                if (skill != null)
                {
                    skill.Activate(entity);
                    handler.InputQueue.Enqueue(new AnimationTrigger()
                    {
                        attackType = AttackType.SpecialAttack,
                        triggerAnimIndex = skill.AnimInfo.AnimIndex,
                        TransitionDuration = skill.AnimInfo.TransitionDuration,
                        TransitionOffset = skill.AnimInfo.TransitionOffset,
                        EndofCurrentAnim = skill.AnimInfo.EndofCurrentAnim

                    });
                }
                else
                    Debug.Log($"{output} not recognize or ability not equipped");


            }).Run();

        }

    }


}