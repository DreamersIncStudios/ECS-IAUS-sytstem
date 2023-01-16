using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Transforms;
using DG.Tweening;
using AISenses.VisionSystems.Combat;
using Stats.Entities;
using DreamersInc.CombatSystem;

namespace DreamersInc.ComboSystem
{
    public partial class ComboInputSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            Entities.WithoutBurst().ForEach(( PlayerComboComponent ComboList, AnimatorComponent anim, Command handler, ref Player_Control PC) =>
            {
                handler.InputQueue = new Queue<AnimationTrigger>();

            }).Run();
        }


        //TODO Decouple this code split into small chu
        protected override void OnUpdate()
        {

            if (!SystemAPI.TryGetSingleton<ControllerInfo>(out var PC))
                return;

        

            Entities.WithoutBurst().ForEach(( PlayerComboComponent ComboList, AnimatorComponent animC, Command handler, ref Player_Control tag) =>
            {

                handler.InputQueue ??= new Queue<AnimationTrigger>();

                if (PC.InSafeZone || PC.Casting || !ComboList.WeaponEquipped)
                {
                    // add logic for play to store weapon

                    return;
                }

                var anim = animC.anim;
                if (!anim.IsInTransition(0) && !ComboList.Combo.ShowMovesPanel)
                {
                    foreach (ComboSingle combotest in ComboList.Combo.ComboLists)
                    {
                        foreach (AnimationCombo comboOption in combotest.ComboList)
                        {
                            if (handler.StateInfo.IsName(comboOption.CurrentStateName.ToString()))
                            {
                                handler.currentStateExitTime = comboOption.AnimationEndTime;
                                if (comboOption.InputAllowed(handler.StateInfo.normalizedTime))
                                {
                                    AnimationTrigger trigger = comboOption.Trigger;
                                    if (combotest.Unlocked && handler.QueueIsEmpty)
                                    {
                                        switch (trigger.attackType)
                                        {
                                            case AttackType.LightAttack:
                                                if (PC.LightAttackb)
                                                {
                                                    handler.InputQueue.Enqueue(trigger);
                                                    PC.ChargedTime = 0.0f;
                                                }
                                                break;
                                            case AttackType.HeavyAttack:
                                                if (PC.HeavyAttackb)
                                                {
                                                    handler.InputQueue.Enqueue(trigger);
                                                    PC.ChargedTime = 0.0f;
                                                }
                                                break;
                                            //TODO Review
                                            case AttackType.ChargedLightAttack:
                                                if (PC.ChargedLightAttackb)
                                                {
                                                    handler.InputQueue.Enqueue(trigger);
                                                    PC.ChargedTime = 0.0f;
                                                }
                                                break;
                                            case AttackType.ChargedHeavyAttack:
                                                if (PC.ChargedHeavyAttackb)
                                                {
                                                    handler.InputQueue.Enqueue(trigger);
                                                    PC.ChargedTime = 0.0f;
                                                }
                                                break;
                                            case AttackType.Projectile:
                                                if (PC.Projectileb)
                                                {
                                                    handler.InputQueue.Enqueue(trigger);
                                                    PC.ChargedTime = 0.0f;
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }).Run();


            Entities.WithStructuralChanges().WithoutBurst().ForEach((Entity entity,AnimatorComponent animC, Command handler, ref AttackTarget attackTarget) =>
            {
                var anim = animC.anim;
                var transform = anim.transform;
                handler.StateInfo = anim.GetCurrentAnimatorStateInfo(0);

                handler.InputQueue ??= new Queue<AnimationTrigger>();
                if (handler.TakeInput)
                {
                    AnimationTrigger temp = handler.InputQueue.Dequeue();
                    if (!anim.GetBool("Weapon In Hand"))
                    {
                        switch (temp.attackType)
                        {
                            case AttackType.LightAttack:
                                anim.CrossFade("Equip_Light", temp.TransitionDuration, 0, temp.TransitionOffset, temp.EndofCurrentAnim);
                                EntityManager.AddComponent<DrawWeapon>(entity);
                                break;
                            case AttackType.HeavyAttack:
                                anim.CrossFade("Equip_Heavy", temp.TransitionDuration, 0, temp.TransitionOffset, temp.EndofCurrentAnim);
                                EntityManager.AddComponent<DrawWeapon>(entity);
                                break;
                            case AttackType.SpecialAttack:
                                anim.CrossFade(temp.TriggerString, temp.TransitionDuration, 0, temp.TransitionOffset, temp.EndofCurrentAnim);
                                EntityManager.AddComponent<DrawWeapon>(entity);
                                break;
                        }

                    }
                    else
                    {
                        anim.CrossFade(temp.TriggerString, temp.TransitionDuration, 0, temp.TransitionOffset, temp.EndofCurrentAnim);

                    }

                    if (!attackTarget.AttackTargetLocation.Equals(new float3(1, 1, 1)))
                    {
                        transform.DOMove(attackTarget.MoveTo(transform.position), .5f, false);
                    }
                    // this need to move to animation event
                }
                if (!anim.IsInTransition(0) && handler.TransitionToLocomotion && !handler.StateInfo.IsTag("Airborne"))
                {
                    if (anim.GetBool("Weapon In Hand"))
                    {
                        if (!handler.BareHands)
                            anim.CrossFade("Locomation_Grounded_Weapon0", .25f, 0, .25f);
                        else
                            anim.CrossFade("Grounded0", .25f, 0, .25f);
                    }
                    else
                        anim.CrossFade("Grounded0", .25f, 0, .25f);

                }
                if (handler.StateInfo.IsName("Unequip")) {
                                EntityManager.AddComponent<StoreWeapon>(entity);
                }
            }).Run();
            Entities.WithoutBurst().WithStructuralChanges().ForEach((Entity entity, Command handler, ref StoreWeapon store) => 
            {
                if (handler.StateInfo.IsName("Grounded0")) {
                    World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EquipSystem>().Update(World.DefaultGameObjectInjectionWorld.Unmanaged);
                } 

            }).Run();

        }

    }
}