using System.Collections;
using UnityEngine;
using DreamersInc.DamageSystem.Interfaces;
using Stats;
using System;
using Random = UnityEngine.Random;
using Dreamers.InventorySystem;
using Stats.Entities;

namespace DreamersInc.DamageSystem
{
    // [RequireComponent(typeof(MeshCollider))]
    public class WeaponDamage : MonoBehaviour, IDamageDealer
    {
        private bool isEquippedToPlayer => transform.root.CompareTag("Player");
        public Action OnHitAction { get; set; }
        public Action ChanceCheck { get; set; }
        public Action CriticalEventCheck { get; set; }
        public Stat Magic_Offense { get; private set; }
        public Stat Range_Offense { get; private set; }
        public Stat Melee_Offense { get; private set; }
        public Attributes Skill { get; private set; }
        public Attributes Speed { get; private set; }

        public int BaseDamage
        {
            get
            {
              // Todo Add mod value for Magic infused/ Ennchanted weapon
                int output = TypeOfDamage switch
                {
                    TypeOfDamage.MagicAoE => Magic_Offense.AdjustBaseValue,
                    TypeOfDamage.Projectile =>  Range_Offense.AdjustBaseValue,
                    TypeOfDamage.Melee => Melee_Offense.AdjustBaseValue,
                    _ => Melee_Offense.AdjustBaseValue,
                };
                return output;
            }
        }
        public float CriticalHitMod => CriticalHit ? Random.Range(1.5f, 2.15f) : 1;
        private float randomMod => Random.Range(.85f, 1.15f);
        
        public bool CriticalHit
        {
            get
            {
                int prob = Mathf.RoundToInt(Random.Range(0, 255));
                int thresold =  (Skill.AdjustBaseValue + Speed.AdjustBaseValue) / 2;
                return prob < thresold;
            }
        }
        public float MagicMod { get; private set; }
        public Element Element { get; private set; }

        public TypeOfDamage TypeOfDamage { get; private set; }

        public bool DoDamage { get; private set; }

        public int DamageAmount()
        {
            return Mathf.RoundToInt(BaseDamage * randomMod );
        }

        public void SetDamageBool(bool value)
        {
            DoDamage = value;
        }

        public void SetDamageType()
        {
            throw new System.NotImplementedException();
        }

        public void SetElement(Element value)
        {
            Element = value;
            //TODO Balance 
            MagicMod =  Element != Element.None ? Magic_Offense.AdjustBaseValue / 10.0f : 1.0f;
        }
        IDamageable self;

        // Use this for initialization
        void Start()
        {


            if (GetComponent<Collider>())
            {
                TypeOfDamage = TypeOfDamage.Melee;
                GetComponent<Collider>().isTrigger = true;
                self = GetComponentInParent<IDamageable>();
            }
            else
            {
                throw new ArgumentNullException(nameof(gameObject), $"Collider has not been setup on equipped weapon. Please set up Collider in Editor; {gameObject.name}");
            }
        }

        float critMod;
        public void CheckChance()
        {
            critMod = CriticalHitMod;
            if (critMod != 1) { 
                CriticalEventCheck();
            }
            ChanceCheck.Invoke();
        }

        public void OnTriggerEnter(Collider other)
        {
            IDamageable hit = other.GetComponent<IDamageable>();
            //Todo add Friend filter.
            if (DoDamage && hit != null && hit != self)
            {
                hit.TakeDamage(DamageAmount(), TypeOfDamage, Element);
                hit.ReactToHit(.5f, transform.root.position, transform.root.forward);
                if(OnHitAction != null)
                    OnHitAction.Invoke();
            }
        }

        public void SetStatData(BaseCharacterComponent stats, TypeOfDamage damageType)
        {
            Magic_Offense = stats.GetStat((int)StatName.Magic_Offence);
            Range_Offense = stats.GetStat((int)StatName.Ranged_Offence);
            Melee_Offense = stats.GetStat((int)StatName.Melee_Offence);
            Speed = stats.GetPrimaryAttribute((int)AttributeName.Speed);
            Skill = stats.GetPrimaryAttribute((int)AttributeName.Skill);
            TypeOfDamage = damageType;
        }
    }
}