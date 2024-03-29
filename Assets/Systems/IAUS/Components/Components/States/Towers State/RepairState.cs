using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS.StateBlobSystem;
using IAUS.ECS.Consideration;

namespace IAUS.ECS.Component
{
    public struct RepairState : IBaseStateScorer
    {
        public BlobAssetReference<AIStateBlobAsset> stateRef { get; set; }
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public ConsiderationScoringData HealthRatio => stateRef.Value.Array[Index].Health;
        public ConsiderationScoringData TargetEnemyInRange => stateRef.Value.Array[Index].DistanceToTargetEnemy;

      [SerializeField]  public ConsiderationScoringData EnergyMana => stateRef.Value.Array[Index].ManaAmmo;
        
        public AIStates Name { get { return AIStates.Heal_Magic; } }

        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }

        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public bool Complete { get; set; } //Todo true when health = max
        public float CoolDownTime { get { return _coolDownTime; } }

        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;

        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

        public float mod { get { return 1.0f - (1.0f / 3.0f); } }
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] public float _resetTime { get; set; }
        [SerializeField] public float _totalScore { get; set; }
    }
    public struct HealSelfTag : IComponentData { }
}
