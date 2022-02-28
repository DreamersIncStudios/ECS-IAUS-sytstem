using DreamersInc.DamageSystem.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
namespace Stats
{
    public class NPCChararacter : BaseCharacter
    {

        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);

            float ModValue = 1.1f;


            var data = new NPCStats() { MaxHealth = MaxHealth, MaxMana = MaxMana, CurHealth = CurHealth, CurMana = CurMana,
                selfEntityRef = entity
            };
            dstManager.AddComponentData(entity, data);
            GetPrimaryAttribute((int)AttributeName.Strength).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Awareness).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Charisma).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Resistance).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.WillPower).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Vitality).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Skill).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Speed).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Luck).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Concentration).BaseValue = (int)(20 * ModValue);
            GetVital((int)VitalName.Health).BaseValue = 50;
            GetVital((int)VitalName.Mana).BaseValue = 25;
            Level = 5;

            StatUpdate();

        }

        public override void TakeDamage(int Amount, TypeOfDamage typeOf, Element element)
        {
            throw new System.NotImplementedException();
        }
    }
}
