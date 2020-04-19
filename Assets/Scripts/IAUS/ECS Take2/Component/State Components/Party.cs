using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using IAUS.ECS.Component;

using IAUS.ECS2.Charaacter;
namespace IAUS.ECS2 {
    [GenerateAuthoringComponent]
    public struct Party :BaseStateScorer
    {

        public ConsiderationData Health;
        public ConsiderationData ThreatInArea;
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _resetTimer;
        [SerializeField] float _resetTime;
        public float _totalScore;
        public float Range;
        public Entity Leader ;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float ResetTimer { get { return _resetTimer; } set { _resetTimer = value; } }
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
    }

    public struct LeaderConsideration: IComponentData
    {
        public int score;
    }
 
    public struct GetLeaderTag : IComponentData { }

    public class GetLeaderSystem : JobComponentSystem
    {
        EntityQueryDesc Leader = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(LocalToWorld), typeof(EliteComponent) },
        };

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeArray<Entity> LeaderEntities = GetEntityQuery(Leader).ToEntityArray(Allocator.TempJob);
            ComponentDataFromEntity<LocalToWorld> positions = GetComponentDataFromEntity<LocalToWorld>(true);
            ComponentDataFromEntity<EliteComponent> elite = GetComponentDataFromEntity<EliteComponent>(false);

            JobHandle getleader = Entities
                .WithDeallocateOnJobCompletion(LeaderEntities)
                .WithNativeDisableParallelForRestriction(positions)
                .WithNativeDisableParallelForRestriction(elite)
                 .ForEach((ref Party party, in LocalToWorld transform, in GetLeaderTag getlead) =>
                 {
                     if (party.Status == ActionStatus.Success)
                         return;
                     Entity TempLeader = party.Leader;
                     if (party.Leader == Entity.Null)
                     {
                         party.Status = ActionStatus.Running;
                         for (int i = 0; i < LeaderEntities.Length; i++)
                         {
                             float dist = Vector3.Distance(transform.Position, positions[LeaderEntities[i]].Position);
                             if (dist < party.Range)
                             {
                                 if (elite[LeaderEntities[i]].IsNotMaxed)
                                 {
                                     TempLeader = party.Leader = LeaderEntities[i];
                                     goto Finish;
                                 }
                             }
                         }
                     }
                         Finish:
                   if(TempLeader!=Entity.Null)
                     { 
                         EliteComponent temp = elite[TempLeader];
                         temp.NumOfSubs++;
                         elite[TempLeader] = temp;
                         party.Status = ActionStatus.Success;
                     }
                 })
                 .WithReadOnly(positions)
                 .Schedule(inputDeps);
            return getleader;
        }
    }
}