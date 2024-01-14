using UnityEngine;
using System.Collections;
using Unity.Entities;
using DreamersInc.DamageSystem.Interfaces;

namespace Stats
{
    [System.Serializable]
    public class PlayerCharacter : BaseCharacter
    {
        public void SetupDataEntity(CharacterClass BaseStats)
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


        public override void TakeDamage(int Amount, TypeOfDamage typeOf, Element element = 0)
        {
            //Todo Figure out element resistances, conditional mods, and possible affinity 
            float defense = typeOf switch
            {
                TypeOfDamage.MagicAoE => MagicDef,
                _ => MeleeDef,
            };

            int damageToProcess = -Mathf.FloorToInt(Amount * defense * Random.Range(.92f, 1.08f));
            AdjustHealth health = new() { Value = damageToProcess };
            World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(SelfEntityRef, health);

        }

        public override void ReactToHit(float impact, Vector3 Test, Vector3 Forward, TypeOfDamage typeOf = TypeOfDamage.Melee, Element element = Element.None)
        {

        }


      
    }
}
