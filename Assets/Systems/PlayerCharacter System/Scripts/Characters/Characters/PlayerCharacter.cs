using UnityEngine;
using System.Collections;
using Unity.Entities;
using DreamersInc.DamageSystem.Interfaces;

namespace Stats
{
    [System.Serializable]
    public class PlayerCharacter : BaseCharacter

    {
        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);
            var data = new PlayerStatComponent() { MaxHealth = MaxHealth, MaxMana = MaxMana, CurHealth = CurHealth, CurMana = CurMana,
                selfEntityRef = entity
            };
            conversionSystem.DeclareAssetDependency(gameObject,this);
            dstManager.AddComponentData(entity, data);

            StatUpdate();
        }

        public override void TakeDamage(int Amount, TypeOfDamage typeOf, Element element = 0)
        {
            //Todo Figure out element resistances, conditional mods, and possible affinity 
            float defense = typeOf switch
            {
                TypeOfDamage.MagicAoE => MagicDef,
                TypeOfDamage.Recovery => 1.0f,
                        
                _ => MeleeDef,
            };
            int damageToProcess = new int();
            if (typeOf != TypeOfDamage.Recovery)
            {
                damageToProcess = -Mathf.FloorToInt(Amount * defense * Random.Range(.92f, 1.08f));
            }
            else
            {
                damageToProcess = Mathf.FloorToInt(Amount * defense );
            }
            AdjustHealth health = new AdjustHealth() { Value = damageToProcess };
            
         World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(SelfEntityRef, health);

        }

        public override void ReactToHit(float impact, Vector3 Test, Vector3 Forward, TypeOfDamage typeOf = TypeOfDamage.Melee, Element element = Element.None)
        {

        }

    }

}
