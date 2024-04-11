using System;
using System.Collections.Generic;
using System.Linq;
//using Dreamers.InventorySystem.AbilitySystem;
using Dreamers.InventorySystem.Interfaces;
using UnityEngine;
namespace DreamersInc.ComboSystem
{
    public interface IAttackSequence
    {
        public enum AttackType
        {
            Melee,Magic,Projectile 
        }

        public List<AnimationTrigger> Triggers { get; }

        public AttackType Type { get; }
        public float ProbabilityWeight { get;  }
        public float ProbabilityPercent { get; set; }

         public float ProbabilityRangeFrom { get; set; }

        public float ProbabilityRangeTo { get; set; }
    }
    [Serializable]
    public class MeleeAttackSequence : IAttackSequence
    {
        public List<AnimationCombo> Attacks;
        public List<AnimationTrigger> Triggers {
            get
            {
                return Attacks.Select(variable => variable.Trigger).ToList();
            }
        }
        public IAttackSequence.AttackType Type =>IAttackSequence.AttackType.Melee;
        public float ProbabilityWeight => probabilityWeight;
        [Range(0, 100)] 
        [SerializeField] private float probabilityWeight;

        public float ProbabilityPercent
        {
            get => probabilityPercent;
            set => probabilityPercent=value;
        }
        

        [SerializeField] private float probabilityPercent;
        // These values are assigned via LootDropTable script. They represent from which number to which number if selected, the item will be picked.
        public float ProbabilityRangeFrom { get; set; }
        public float ProbabilityRangeTo { get; set; }
    }

    [Serializable]
    public class MagicAttackSequence : IAttackSequence
    {
        public List<AnimationCombo> Attacks;
        public List<AnimationTrigger> Triggers {
            get
            {
                return Attacks.Select(variable => variable.Trigger).ToList();
            }
        }
       // public AbilitySO Ability;
        public IAttackSequence.AttackType Type =>IAttackSequence.AttackType.Magic;
        public float ProbabilityWeight => probabilityWeight;
        [Range(0, 100)] 
        [SerializeField] private float probabilityWeight;

 
        public float ProbabilityPercent
        {
            get => probabilityPercent;
            set => probabilityPercent=value;
        }
        

        [SerializeField] private float probabilityPercent;
        
        // These values are assigned via LootDropTable script. They represent from which number to which number if selected, the item will be picked.
        public float ProbabilityRangeFrom { get; set; }
        public float ProbabilityRangeTo { get; set; }
    }
    
    [Serializable]
    public class ProjectileAttackSequence : IAttackSequence
    {
        public List<AnimationCombo> Attacks;
        public List<AnimationTrigger> Triggers {
            get
            {
                return Attacks.Select(variable => variable.Trigger).ToList();
            }
        }
        public ItemBaseSO ItemToShoot;
        public IAttackSequence.AttackType Type =>IAttackSequence.AttackType.Projectile;

        public float ProbabilityWeight => probabilityWeight;
        [Range(0, 100)] 
        [SerializeField] private float probabilityWeight;

        public float ProbabilityPercent
        {
            get => probabilityPercent;
            set => probabilityPercent=value;
        }
        

        [SerializeField] private float probabilityPercent;  

        // These values are assigned via LootDropTable script. They represent from which number to which number if selected, the item will be picked.

        public float ProbabilityRangeFrom { get; set; }
        public float ProbabilityRangeTo { get; set; }
    }
  
    
}