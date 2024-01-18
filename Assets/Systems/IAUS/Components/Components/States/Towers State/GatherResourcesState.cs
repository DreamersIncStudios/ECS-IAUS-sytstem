using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS.StateBlobSystem;
using IAUS.ECS.Consideration;
using Unity.Collections;
using Unity.Burst.Intrinsics;

namespace IAUS.ECS.Component
{
    public struct GatherResourcesState : IBaseStateScorer
    {
        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public int Index { get; set; }
       public ConsiderationScoringData HealthRatio => stateRef.Value.Array[Index].Health;
        public ConsiderationScoringData TargetEnemyInRange => stateRef.Value.Array[Index].DistanceToTargetEnemy;

        public AIStates name { get { return AIStates.GatherResources; } }

        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }

        public ActionStatus Status { get { return _status; } set { _status = value; } }

        public float CoolDownTime { get { return _coolDownTime; } }

        public bool Complete; //Todo true when level time runs out 
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;

        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

        public float mod { get { return 1.0f - (1.0f / 2.0f); } }
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] public float _resetTime { get; set; }
        [SerializeField] public float _totalScore { get; set; }

    }

    public struct GatherResourcesTag : IComponentData {  }

   
}