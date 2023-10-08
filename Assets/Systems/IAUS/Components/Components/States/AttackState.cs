using System.Linq;
using DreamersInc.ComboSystem;
using IAUS.ECS.Consideration;
using IAUS.ECS.StateBlobSystem;
using Sirenix.Utilities;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public struct AttackState : IBaseStateScorer
    {
        public AttackState(float coolDownTime, bool melee = false, bool magic = false, bool range = false)
        {
            this.coolDownTime = coolDownTime;
            CapableOfMelee = melee;
            CapableOfMagic = magic;
            CapableOfProjectile = range;
            status = ActionStatus.Idle;
            Index = 0;
            resetTime = 0;
            totalScore = 0;
        }

        public bool CapableOfMelee;
        public bool CapableOfMagic;
        public bool CapableOfProjectile;
        public void SetIndex(int index)
        {
            Index = index;
        }

        public int Index { get; private set; }
        public AIStates Name => AIStates.Attack;
        
        public float TotalScore { get => totalScore;
            set { totalScore = value; } }
        public ActionStatus Status { get { return status; } set { status = value; } }
        public float CoolDownTime { get { return coolDownTime; } }
        public bool InCooldown => Status == ActionStatus.CoolDown;
        public float ResetTime { get { return resetTime; } set { resetTime = value; } }
        public float mod { get { return 1.0f - (1.0f / 4.0f); } }

          float coolDownTime;
         float resetTime { get; set; }
         float totalScore { get; set; }
         ActionStatus status;
  
    }
    public struct AttackActionTag : IComponentData {
        public int SubStateNumber;
    }
    public struct MeleeAttackSubState : IComponentData {
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public float AttackRange { get; set; } //Todo Pull from character stats speed
        public AIStates Name => AIStates.AttackMelee;
        public float AttackDelay;
       [SerializeField] public bool AttackNow => AttackDelay <= 0.0f;
        public float mod { get { return 1.0f - (1.0f / 3.0f); } }
        FixedList512Bytes<AIComboInfo> unlockedMoves;

        public void SetupPossibleAttacks(ComboSO combo)
        {
            unlockedMoves = new FixedList512Bytes<AIComboInfo>();
            foreach (var item in combo.ComboLists.Where(item => item.Unlocked && !item.AnimationList.IsNullOrEmpty()))
            {
                unlockedMoves.Add(new AIComboInfo()
                {
                    AttackName = item.Name,
                    Chance =  (int)item.AnimationList[0].Trigger.Chance,
                    Trigger =  item.AnimationList[0].Trigger
                });
            }
        }

        public  int SelectAttackIndex(uint seed) {
                //Todo updated solution using LootBox system
                var maxRange = unlockedMoves.Length;
                AttackDelay = Unity.Mathematics.Random.CreateFromIndex(seed).NextFloat(4, 15);
                return Unity.Mathematics.Random.CreateFromIndex(seed).NextInt(0, maxRange);
        }

        public AnimationTrigger GetAnimationTrigger(int index)
        {
            return unlockedMoves[index].Trigger;
        }

    }
    public struct MagicAttackSubState : IComponentData
    {
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public static AIStates Name => AIStates.AttackMagic;
        public float mod { get { return 1.0f - (1.0f / 5.0f); } }
        public void SetupPossibleAttacks(){}
        
    }
    public struct MagicMeleeAttackSubState : IComponentData
    {
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public static AIStates Name => AIStates.AttackMagicMelee;

        public float Mod => 1.0f - (1.0f / 4.0f);
        public void SetupPossibleAttacks(){}
        
    }
    public struct RangedAttackSubState : IComponentData
    {
        public float MaxEffectiveRange;
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public static AIStates Name => AIStates.AttackRange;
        
       public float Mod => 1.0f - (1.0f / 5.0f);
        public void SetupPossibleAttacks(){}

    }

    public struct MeleeAttackTag : IComponentData
    {
        public int AttackIndex;
        public int PositionIndex;

    }
    public struct MagicAttackTag : IComponentData { }
    public struct RangeAttackTag : IComponentData { }
    public struct MagicMeleeAttackTag : IComponentData { }


    public enum SubAttackStates { }
}