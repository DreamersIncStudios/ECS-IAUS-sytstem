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
            float ModValue = 1.1f;
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

            base.Convert(entity, dstManager, conversionSystem);
            var data = new NPCStats()
            {
                MaxHealth = MaxHealth,
                MaxMana = MaxMana,
                CurHealth = CurHealth,
                CurMana = CurMana,
                selfEntityRef = entity
            };
            dstManager.AddComponentData(entity, data);
            Level = 5;


            StatUpdate();

        }

        Rigidbody rb;
        Animator anim;
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            anim = GetComponent<Animator>();
        }

        public override void ReactToDamage(Vector3 DirOfAttack)
        {
            throw new System.NotImplementedException();
        }

        public override void TakeDamage(int Amount, TypeOfDamage typeOf, Element element)
        {
            base.TakeDamage(Amount, typeOf, element);
            //Todo Add system so that NPC can not hit 0 HP ?????


        }
    }
}
