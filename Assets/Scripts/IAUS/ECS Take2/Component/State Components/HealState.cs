using Unity.Entities;
using UnityEngine;
using IAUS.Core;
using Unity.Jobs;

namespace IAUS.ECS2 {
    public struct HealSelfState : BaseStateScorer
    {
        public ConsiderationData Health;
        public ConsiderationData ThreatInArea;
        public ConsiderationData TimeBetweenHeals;
        public ConsiderationData RecoveryItemsInInventory;

        
        public float mod { get { return 1.0f - (1.0f / 3.0f); } }

        [SerializeField] ActionStatus _status;
        [SerializeField] public float _resetTimer;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;
        public float _timeBetweenHeals;
        public float timerForHealing;
        public int FullInventoryofItem;

        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float ResetTimer { get { return _resetTimer; } set { _resetTimer = value; } }
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
    }

    [UpdateInGroup(typeof(IAUS_UpdateScore))]
    public class HealSelfStateScore : SystemBase
    {
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = Entities.ForEach((ref HealSelfState state, in HealthConsideration health,in ThreatInAreaConsideration Threat) =>
            {
                float TotalScore = Mathf.Clamp01(state.Health.Output(health.Ratio)*
                    state.TimeBetweenHeals.Output(state.timerForHealing/state._timeBetweenHeals)
                     )
                 ;
               state.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * state.mod) * TotalScore);


            }
            ).ScheduleParallel(systemDeps);

            Dependency = systemDeps;
        }
    }





    [Unity.Burst.BurstCompile]
    public struct HealSelf : IJobChunk
    {
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            throw new System.NotImplementedException();
        }
    }
}
