using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using DreamersInc.DamageSystem.Interfaces;
namespace Stats
{
    public class EnemyCharacter : BaseCharacter
    {
        public uint EXPgained;
        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            float ModValue = 1.1f;
            GetPrimaryAttribute((int)AttributeName.Strength).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Awareness).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Charisma).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Resistance).BaseValue = (int)(20* ModValue);
            GetPrimaryAttribute((int)AttributeName.WillPower).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Vitality).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Skill).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Speed).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Luck).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Concentration).BaseValue = (int)(20 * ModValue);
            GetVital((int)VitalName.Health).BaseValue = 50;
            GetVital((int)VitalName.Mana).BaseValue = 25;

            base.Convert(entity, dstManager, conversionSystem);
            var data = new EnemyStats() { MaxHealth = MaxHealth, MaxMana = MaxMana, CurHealth = CurHealth, CurMana = CurMana,
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
            //Todo pass in force and add resistance to change 
            if (Mathf.Abs(DirOfAttack.z) > Mathf.Abs(DirOfAttack.x))
            {
                rb.AddForce(900 * DirOfAttack.normalized.z * this.transform.forward, ForceMode.Impulse);
                if (anim)// Todo Create anim class of non animation damagable objects 
                {
                    if (DirOfAttack.normalized.z > 0)
                    {
                        //Todo break IAUS
                        anim.Play("Small Hit Front");
                    }
                    else
                    {
                        anim.Play("Small Hit Back");
                    }
                }
            }
            else
            {
                rb.AddForce(900 * DirOfAttack.x * this.transform.right, ForceMode.Impulse);
                if (anim)
                {
                    if (DirOfAttack.normalized.x > 0)
                    {
                        anim.Play("Small Hit Right");
                    }
                    else
                    {
                        anim.Play("Small Hit Left");
                    }
                }
            }
        }

        public override void TakeDamage(int Amount, TypeOfDamage typeOf, Element element)
        {
            //Todo add filter for same faction attacks
            base.TakeDamage(Amount, typeOf, element);
        }

    }
}
