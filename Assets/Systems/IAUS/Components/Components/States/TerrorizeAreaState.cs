using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using AISenses;
using AISenses.VisionSystems;
using Unity.Collections;

namespace IAUS.ECS.Component
{
    public struct TerrorizeAreaState : IBaseStateScorer
    {
        public float3 TargetPosition;
        public Entity TargetEntity;
        public int TargetPositionID;
        public TerrorizeAreaState(float coolDownTime, bool melee = false, bool magic = false, bool range = false)
        {
            this.coolDownTime = coolDownTime;
            _status = ActionStatus.Idle;
            Index = 0;
            _resetTime = 0;
            _totalScore = 0;
            CapableOfMelee = melee;
            CapableOfMagic = magic;
            CapableOfProjectile = range;
            AttackResetTimer = 0.0f;
            TargetPosition = float3.zero;
            TargetEntity = Entity.Null;
            AttackPlans = new FixedList64Bytes<AttackPlan>();
            TargetPositionID = -1;
        }

        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public AIStates Name { get { return AIStates.Terrorize; } }
 
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return coolDownTime; } }
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
    
        public float mod { get { return 1.0f - (1.0f / 3.0f); } }
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float coolDownTime;
        [SerializeField] public float _resetTime { get; set; }
        [SerializeField] public float _totalScore { get; set; }
        
        public  FixedList32Bytes<AttackPlan> AttackPlans;
        public float AttackResetTimer;
        [SerializeField]  public bool InAttackCooldown => AttackResetTimer != 0.0f;
        public bool CapableOfMelee, CapableOfMagic,CapableOfProjectile;
    }


    public struct TerrorizeAreaTag : IComponentData {

    }
}