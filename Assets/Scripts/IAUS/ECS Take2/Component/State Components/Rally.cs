using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using IAUS.Core;
using Unity.Transforms;
using Unity.Collections;
using IAUS.ECS.Component;
using Unity.Mathematics;
using IAUS.ECS2.Charaacter;
namespace IAUS.ECS2
{ using InfluenceMap;
    [GenerateAuthoringComponent]
    public struct Rally : BaseStateScorer
    {
        public ConsiderationData Health;
        public ConsiderationData ThreatInArea;
        public ConsiderationData DistanceToLeader;
        public ConsiderationData HaveLeader;
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _resetTimer;
        [SerializeField] float _resetTime;
        public float _totalScore;

        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float ResetTimer { get { return _resetTimer; } set { _resetTimer = value; } }
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        public float3 RallyPoint;
        public bool SetPosition { get; set; }
        public bool Rallied { get; set; }
    }

    public struct RallyActionTag : IComponentData { }

    [UpdateInGroup(typeof(IAUS_UpdateScore))]

    public class RallyScoreSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float DT = Time.DeltaTime;
            JobHandle RallyScoreUpdate = Entities.ForEach((ref Rally rally, ref Patrol patrol, in Party party, in HealthConsideration health, in DetectionConsideration detectConsider,
                in LeaderConsideration LeaderCon
                ) =>
            {

                if (rally.Status != ActionStatus.Running)
                {
                    switch (rally.Status)
                    {
                        case ActionStatus.CoolDown:
                            if (rally.ResetTime > 0.0f)
                            {
                                rally.ResetTime -= DT;
                            }
                            else
                            {
                                rally.Status = ActionStatus.Idle;
                                rally.ResetTime = 0.0f;
                            }
                            break;
                        case ActionStatus.Failure:
                            rally.ResetTime = rally.ResetTimer / 2.0f;
                            rally.Status = ActionStatus.CoolDown;
                            break;
                        case ActionStatus.Interrupted:
                            rally.ResetTime = rally.ResetTimer / 2.0f;
                            rally.Status = ActionStatus.CoolDown;

                            break;
                        case ActionStatus.Success:
                            rally.ResetTime = rally.ResetTimer;
                            rally.Status = ActionStatus.CoolDown;
                            patrol.CanPatrol = true;
                            patrol.Status = ActionStatus.Idle;
                            break;
                    }
                }
                //add math.clamp01
                // make sure all outputs goto zero
                if (!rally.Rallied)
                {
                    float mod = 1.0f - (1.0f / 2.0f);
                    float TotalScore = Mathf.Clamp01(rally.Health.Output(health.Ratio) * rally.DistanceToLeader.Output(party.DistanceLeaderScore) *
                        rally.HaveLeader.Output(LeaderCon.score) * rally.ThreatInArea.Output(detectConsider.ThreatInArea));

                    rally.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * mod) * TotalScore);
                }
                else
                    rally.TotalScore = 0.0f;
            }).Schedule(inputDeps);
            return RallyScoreUpdate;
        }

        
    }

    [UpdateInGroup(typeof(IAUS_UpdateState))]
    public class GotoRallyPointSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle rallyAtLeader = Entities.ForEach((ref Rally rally, ref Movement move, ref InfluenceValues InfluValues,
                in Party party, in RallyActionTag tag) => 
            {


                 if (rally.SetPosition)
                {
                rally.Status = ActionStatus.Running;
                    move.TargetLocation = rally.RallyPoint;
                    InfluValues.TargetLocation = rally.RallyPoint;
                    move.Completed = false;
                    move.CanMove = true;
                    move.SetTargetLocation = true;
                    rally.SetPosition = false;
                }

            //complete
            if (rally.Status == ActionStatus.Running)
                {
                    if (move.Completed && !move.CanMove)
                    {
                        rally.Status = ActionStatus.Success;
                        rally.Rallied = true;
                    }
                }
            })
                .Schedule(inputDeps);
            return rallyAtLeader    ;
        }
    }
   
}
