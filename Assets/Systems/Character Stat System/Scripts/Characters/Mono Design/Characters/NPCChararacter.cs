using DreamersInc.DamageSystem.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using DreamersInc.CombatSystem.Animation;

namespace Stats
{
    public class NPCChararacter : BaseCharacter
    {


        public override void TakeDamage(int Amount, TypeOfDamage typeOf, Element element)
        {
            //Todo Figure out element resistances, conditional mods, and possible affinity 
            float defense = typeOf switch
            {
                TypeOfDamage.MagicAoE => MagicDef,
                TypeOfDamage.Melee => MeleeDef,
                _ => MeleeDef,
            };
            Debug.Log((float)Amount / defense);
            int damageToProcess = -Mathf.FloorToInt(Amount * defense * Random.Range(.92f, 1.08f));
            AdjustHealth health = new AdjustHealth() { Value = damageToProcess };
            World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(SelfEntityRef, health);
        }
        public override void ReactToHit(float impact, Vector3 Test, Vector3 Forward, TypeOfDamage typeOf = TypeOfDamage.Melee , Element element= Element.None)
        {
            //Todo Figure out element resistances, conditional mods, and possible affinity 
            float defense = typeOf switch
            {
                TypeOfDamage.MagicAoE => MagicDef,
                TypeOfDamage.Melee => MeleeDef,
                _ => MeleeDef,
            };

            ReactToContact reactTo = new ReactToContact() { 
                ForwardVector = Forward,
                positionVector = this.transform.position,
                RightVector = transform.right,
                HitIntensity = Mathf.FloorToInt( impact / (defense * 10.0f) * Random.Range(.92f, 1.08f)),
                HitContactPoint =Test
            };
            World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(SelfEntityRef, reactTo);

        }
    }
}
