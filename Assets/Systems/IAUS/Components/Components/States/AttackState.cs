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
        public AttackState(float coolDownTime, bool melee, bool magic, bool range)
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

        public float mod { get { return 1.0f - (1.0f / 3.0f); } }

        public void SetupPossibleAttacks(ComboSO combo)
        {
            UnlockedMoves = new FixedList512Bytes<AIComboInfo>();
            foreach (var item in combo.ComboLists)
            {
                if(item.Unlocked && !item.AnimationList.IsNullOrEmpty())
                    UnlockedMoves.Add(new AIComboInfo()
                    {
                        AttackName = item.Name,
                        Chance =  (int)item.AnimationList[0].Trigger.Chance,
                        Trigger =  item.AnimationList[0].Trigger
                    });
                
            }
        }

        public int SelectAttackIndex {
            get
            {
                //Todo updated solution using LootBox system
                var maxRange = UnlockedMoves.Length;
                return Mathf.RoundToInt(Random.Range(0, maxRange));

            }
        }
        public FixedList512Bytes<AIComboInfo> UnlockedMoves;
    }
    public struct MagicAttackSubState : IComponentData
    {
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public static AIStates Name => AIStates.AttackMagic;

       // public ConsiderationScoringData TargetInRange => StateRef.Value.Array[Index].DistanceToTarget;
        //public ConsiderationScoringData Mana => StateRef.Value.Array[Index].ManaAmmo;
        //public ConsiderationScoringData CoverInRange => StateRef.Value.Array[Index].DistanceToPlaceOfInterest;
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
        public BlobAssetReference<AIStateBlobAsset> StateRef;
        public static AIStates Name => AIStates.AttackRange;

      //  public ConsiderationScoringData TargetInRange => StateRef.Value.Array[Index].DistanceToTarget;
      //  public ConsiderationScoringData Ammo => StateRef.Value.Array[Index].ManaAmmo;
      //  public ConsiderationScoringData CoverInRange => StateRef.Value.Array[Index].DistanceToPlaceOfInterest;
      //  public float Mod => 1.0f - (1.0f / 5.0f);
        public void SetupPossibleAttacks(){}

    }
    public struct MeleeAttackTag : IComponentData { }
    public struct MagicAttackTag : IComponentData { }
    public struct RangeAttackTag : IComponentData { }
    public struct MagicMeleeAttackTag : IComponentData { }


    public enum SubAttackStates { }
}