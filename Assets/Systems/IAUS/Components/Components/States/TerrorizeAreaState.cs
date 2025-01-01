using IAUS.ECS.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS.StateBlobSystem;
using IAUS.ECS.Consideration;
using Unity.Mathematics;
using AISenses;

namespace IAUS.ECS.Component
{
    public struct TerrorizeAreaState : IBaseStateScorer
    {

        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public AIStates Name { get { return AIStates.Terrorize; } }
        public TerrorizeSubstates terrorizeSubstate;
        public float MaxTerrorizeRadius;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; } }
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        public Target attackThis;
        public bool HasAttack => attackThis.CanSee;
    
        public float mod { get { return 1.0f - (1.0f / 3.0f); } }
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] public float _resetTime { get; set; }
        [SerializeField] public float _totalScore { get; set; }
    }
    public enum TerrorizeSubstates { None, FindTarget, MoveToTarget, AttackTarget,  }

    public struct TerrorizeAreaTag : IComponentData {
        public TerrorizeSubstates CurSubState;
    }
}