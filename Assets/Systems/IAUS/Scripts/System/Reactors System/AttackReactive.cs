using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using DreamersInc.ComboSystem.NPC;
using DreamersInc.ComboSystem;
using Components.MovementSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<AttackActionTag,AttackTargetState, IAUS.ECS.Systems.Reactive.AttackTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, IAUS.ECS.Systems.Reactive.AttackTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, IAUS.ECS.Systems.Reactive.AttackTagReactor>.ManageComponentRemovalJob))]


namespace IAUS.ECS.Systems.Reactive
{
    public struct AttackTagReactor : IComponentReactorTagsForAIStates<AttackActionTag, AttackTargetState>
    {
        public void ComponentAdded(Entity entity, ref AttackActionTag newComponent, ref AttackTargetState AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
            newComponent.StyleOfAttack = AIStateCompoment.HighScoreAttack.style; 
        }

        public void ComponentRemoved(Entity entity, ref AttackTargetState AIStateCompoment, in AttackActionTag oldComponent)
        {
            if ( AIStateCompoment.Status == ActionStatus.Success)
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = 5.0f; // TODO assign in editor 
            }
            else
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = 7.5f ;

            }
        }

        public void ComponentValueChanged(Entity entity, ref AttackActionTag newComponent, ref AttackTargetState AIStateCompoment, in AttackActionTag oldComponent)
        {
        }

        public class AttackReactiveSystem : AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>
        {
            protected override AttackTagReactor CreateComponentReactor()
            {
                return new AttackTagReactor();
            }
        }

    }
    [UpdateAfter(typeof(PatrolMovement))]
    public class AttackSystem : ComponentSystem
    {
        EntityQuery AttackAdded;
        EntityQuery AttackRemoved;
        EntityQuery AttackTime;
        EntityQuery ActiveAttack;


        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            AttackAdded = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTargetState)), ComponentType.ReadWrite(typeof(AttackActionTag)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(AttackTypeInfo))
                , ComponentType.ReadOnly(typeof(LocalToWorld))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>.StateComponent)) }
            });
            AttackRemoved = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTargetState)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(AttackTypeInfo))
                , ComponentType.ReadOnly(typeof(LocalToWorld)),ComponentType.ReadOnly(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>.StateComponent))
                },
                None = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackActionTag)) }
            });
            AttackTime = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTargetState)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(AttackTypeInfo))
                , ComponentType.ReadOnly(typeof(LocalToWorld)),ComponentType.ReadOnly(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>.StateComponent))
              , ComponentType.ReadWrite(typeof(AttackActionTag)) },
            });
            ActiveAttack = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTargetState)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(AttackTypeInfo))
                , ComponentType.ReadOnly(typeof(LocalToWorld)),ComponentType.ReadOnly(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>.StateComponent))
              , ComponentType.ReadWrite(typeof(AttackActionTag)) },
            });


        }

        protected override void OnUpdate()
        {
            BufferFromEntity<NPCAttackBuffer> bufferFromEntity = GetBufferFromEntity<NPCAttackBuffer>();

            Entities.WithNone(ComponentType.ReadOnly(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>.StateComponent))).
                ForEach((Entity entity, ref AttackActionTag tag, NPCComboComponent comboList, Command handler) => {
                DynamicBuffer<NPCAttackBuffer> buffer = bufferFromEntity[entity];
                    //    int index =comboList.combo.GetAnimationComboIndex(handler.StateInfo);
                    //restart:

                    //float indexPicked = Random.Range(0, comboList.combo.GetMaxProbAtCurrentState(index));

                    //foreach (var item in comboList.combo.ComboList[index].Triggers)
                    //{
                    //    if (item.Picked(indexPicked))
                    //    {

                    //        buffer.Add(new NPCAttackBuffer()
                    //        {
                    //            Trigger = item
                    //        });
                    //        if (item.AttackAgain(Random.Range(2, 100)))
                    //        {
                    //            index = comboList.combo.GetAnimationComboIndex(item.TriggeredAnimName);
                    //            goto restart;
                    //        }
                    //    }
                    //}

                    //Todo use for IAUS Testing only
                    buffer.Add(new NPCAttackBuffer()
                    {
                        Trigger = new AnimationTrigger()
                        {
                            Type = AttackType.LightAttack
                        }
                    }); ;
            });

            Entities.ForEach((DynamicBuffer<NPCAttackBuffer> A, Command handler) => {
                if (!A.IsEmpty)
                {
                    switch (A[0].Trigger.Type) {
                        case AttackType.LightAttack:
                        case AttackType.HeavyAttack:
                            //Move to attack range then trigger animation;
                            break;
                        case AttackType.Projectile:
                            //Rotate Target and then trigger attack 

                            break;

                    }
                    if (A[0].Trigger.trigger)
                    {
                        handler.InputQueue.Enqueue(A[0].Trigger);
                        A.RemoveAt(0);
                    }
                    else
                    {
                        A[0].Trigger.AdjustTime(Time.DeltaTime);
                    }
                }
            });
        }
    }


}