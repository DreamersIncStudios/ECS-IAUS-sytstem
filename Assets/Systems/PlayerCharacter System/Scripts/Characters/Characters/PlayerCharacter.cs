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
            dstManager.AddComponentData(entity, data);
            StatUpdate();
        }
        Rigidbody rb;
        Animator anim;
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            anim = GetComponent<Animator>();
        }


        public override void TakeDamage(int Amount, TypeOfDamage typeOf, Element element)
        {
            base.TakeDamage(Amount, typeOf, element);
        }
        public override void ReactToDamage(Vector3 DirOfAttack)
        {
            //Todo pass in force and add resistance to change 
            if (Mathf.Abs(DirOfAttack.z) > Mathf.Abs(DirOfAttack.x))
            {
                rb.AddForce(900 * DirOfAttack.normalized.z * this.transform.forward, ForceMode.Impulse);
                if (DirOfAttack.normalized.z > 0)
                {
                    anim.Play("Small Hit Front");
                }
                else
                {
                    anim.Play("Small Hit Back");
                }
            }
            else
            {
                rb.AddForce(900 * DirOfAttack.x * this.transform.right, ForceMode.Impulse);
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

}
