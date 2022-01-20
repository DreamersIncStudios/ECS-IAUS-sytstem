using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS.StateBlobSystem;
using IAUS.ECS.Consideration;

namespace IAUS.ECS.Component
{
    public struct GatherResourcesState : IBaseStateScorer
    {
        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public int Index;
        public ConsiderationScoringData HealthRatio => stateRef.Value.Array[Index].Health;
        public ConsiderationScoringData TargetInRange => stateRef.Value.Array[Index].TargetInRange;

        public AIStates name { get { return AIStates.GatherResources; } }

        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }

        public ActionStatus Status { get { return _status; } set { _status = value; } }

        public float CoolDownTime { get { return _coolDownTime; } }

        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;

        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

        public float mod { get { return 1.0f - (1.0f / 2.0f); } }
        [HideInInspector] public bool UpdatePatrolPoints;
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;

    }

    public struct GatherResourcesTag : IComponentData { }
}