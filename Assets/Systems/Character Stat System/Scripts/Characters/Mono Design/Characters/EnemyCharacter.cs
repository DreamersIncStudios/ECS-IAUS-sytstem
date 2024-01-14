using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using DreamersInc.DamageSystem.Interfaces;
using DreamersInc.CombatSystem.Animation;

namespace Stats
{
    public class EnemyCharacter : BaseCharacter
    {
        public uint EXPgained;
        public CharacterClass BaseStats;

        public void SetupDataEntity()
        {
            //Todo get level and stat data
            Init();
            this.Level = BaseStats.Level;
            float ModValue = BaseStats.LevelMod;
            this.GetPrimaryAttribute((int)AttributeName.Strength).BaseValue = (int)(BaseStats.Strength * ModValue);
            this.GetPrimaryAttribute((int)AttributeName.Awareness).BaseValue = (int)(BaseStats.Awareness * ModValue);
            this.GetPrimaryAttribute((int)AttributeName.Charisma).BaseValue = (int)(BaseStats.Charisma * ModValue);
            this.GetPrimaryAttribute((int)AttributeName.Resistance).BaseValue = (int)(BaseStats.Resistance * ModValue);
            this.GetPrimaryAttribute((int)AttributeName.WillPower).BaseValue = (int)(BaseStats.WillPower * ModValue);
            this.GetPrimaryAttribute((int)AttributeName.Vitality).BaseValue = (int)(BaseStats.Vitality * ModValue);
            this.GetPrimaryAttribute((int)AttributeName.Skill).BaseValue = (int)(BaseStats.Skill * ModValue);
            this.GetPrimaryAttribute((int)AttributeName.Speed).BaseValue = (int)(BaseStats.Speed * ModValue);
            this.GetPrimaryAttribute((int)AttributeName.Luck).BaseValue = (int)(BaseStats.Luck * ModValue);
            this.GetPrimaryAttribute((int)AttributeName.Concentration).BaseValue = (int)(BaseStats.Concentration * ModValue);
            this.GetVital((int)VitalName.Health).StartValue = 500;
            this.GetVital((int)VitalName.Mana).StartValue = 250;
            StatUpdate();

        }

        public override void TakeDamage(int Amount, TypeOfDamage typeOf, Element element)
        {
            //Todo Figure out element resistances, conditional mods, and possible affinity 
            float defense = typeOf switch
            {
                TypeOfDamage.MagicAoE => MagicDef,
                _ => MeleeDef,
            };

            int damageToProcess = -Mathf.FloorToInt(Amount * defense * Random.Range(.92f, 1.08f));
            // Debug.Log(damageToProcess + " HP of damage to target "+ Name);
            AdjustHealth health = new() { Value = damageToProcess };
            World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(SelfEntityRef, health);
        }

        public override void ReactToHit(float impact, Vector3 Test, Vector3 Forward, TypeOfDamage typeOf = TypeOfDamage.Melee, Element element = Element.None)
        {
            //Todo Figure out element resistances, conditional mods, and possible affinity 
            float defense = typeOf switch
            {
                TypeOfDamage.MagicAoE => MagicDef,
                TypeOfDamage.Melee => MeleeDef,
                _ => MeleeDef,
            };
           
            ReactToContact reactTo = new()
            {
                ForwardVector = Forward,
                positionVector = this.transform.position,
                RightVector = transform.right,
                HitIntensity = 4.45f,//Todo balance the mathe Mathf.FloorToInt(impact / (defense * 10.0f) * Random.Range(.92f, 1.08f)),
                HitContactPoint = Test
            };
            if (!World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<ReactToContact>(SelfEntityRef))
                World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(SelfEntityRef, reactTo);
        }

    }


}
