using UnityEngine;
using Unity.Entities;
using System;
using IAUS.ECS.Consideration;
using IAUS.ECS.StateBlobSystem;

namespace IAUS.ECS.Component {
    [Serializable]
    public struct Wait : IBaseStateScorer
    {
        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public int Index;

        public AIStates name { get { return AIStates.Wait; } }
        public ConsiderationScoringData TimeLeft { get { return  stateRef.Value.Array[Index].Timer; } }
        public ConsiderationScoringData HealthRatio { get { return stateRef.Value.Array[Index].Health; } }
        /// <summary>
        /// Utility score for Attackable target in Ranges
        /// </summary>
        public ConsiderationScoringData TargetInRange => stateRef.Value.Array[Index].DistanceToTarget;

       [SerializeField] public bool Complete => Timer <= 0.0f;
        /// <summary>
        /// How much time NPC has left to wait at location.
        /// </summary>
        public float Timer { get; set; }
        public float StartTime;
        public float TimePercent => Mathf.Clamp01( Timer / StartTime);
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; }  }
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;

        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        public float mod { get { return 1.0f - (1.0f / 3.0f); } }
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] public float _resetTime { get; set; }
        [SerializeField] public float _totalScore { get; set; }
    }
    [Serializable]
    public struct WaitBuilderData {
        public float StartTime;
        public float CoolDownTime;
    }

    public struct WaitActionTag : IComponentData
    {
        public bool tester;
    }

    //TODO Move to better file location 
    public enum Difficulty { Easy, Normal, Hard }
    public enum NPCLevel
    {
        Grunt, Specialist,Tower, NPC, Daemon

    }


}
