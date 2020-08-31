using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using IAUS.Core;


// Consider deleting ???
using IAUS.ECS2.Character;
namespace IAUS.ECS2 {
    [GenerateAuthoringComponent]
    public struct Party : BaseStateScorer
    {
        [Header("Considerations")]
        public ConsiderationData Health;
        public ConsiderationData ThreatInArea;
        public ConsiderationData HaveLeader;
        public float mod { get { return 1.0f - (1.0f / 3.0f); } }

        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _resetTimer;
        [SerializeField] float _resetTime;

        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float ResetTimer { get { return _resetTimer; } set { _resetTimer = value; } }
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

        [Header("Data")]
        public float _totalScore;
        public float Range;
        public float MaxDistanceFromLeader;
        public Entity Leader;
        [HideInInspector] public float distanceToLeader;
        public float DistanceLeaderScore { get { return Mathf.Clamp01(distanceToLeader / MaxDistanceFromLeader);  } }
    }

    public struct LeaderConsideration: IComponentData
    {
        public int score;
    }
 
    public struct GetLeaderTag : IComponentData { }
    
    [UpdateInGroup(typeof(IAUS_UpdateSystem))]

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
                 .ForEach((ref Party party, ref Patrol patrol, ref RetreatToLocation rally, in LocalToWorld transform, in GetLeaderTag getlead) =>
                 {
                     if (party.Status == ActionStatus.Success)
                         return;

                     Entity TempLeader = party.Leader;
                     if (party.Leader == Entity.Null)
                     {
                         patrol.CanPatrol = false;
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
                     if (TempLeader != Entity.Null)
                     {
                         EliteComponent temp = elite[TempLeader];
                         temp.NumOfSubs++;
                         elite[TempLeader] = temp;
                         party.Status = ActionStatus.Success;
                         rally.RallyPoint = positions[party.Leader].Position;
                         rally.SetPosition = true;
                     }
                 })
                 .WithReadOnly(positions)
                 .Schedule(inputDeps);
           

            return getleader;
        }
    }

    [UpdateInGroup(typeof(IAUS_UpdateConsideration))]
    public class UpdateDistanceToLeader : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ComponentDataFromEntity<LocalToWorld> position = GetComponentDataFromEntity<LocalToWorld>(false);
            JobHandle DistanceUpdate = Entities
                .WithNativeDisableParallelForRestriction(position)
                .WithReadOnly(position)
                 .ForEach((ref Party party,  in LocalToWorld toWorld) => 
                 {
                     if (party.Leader != Entity.Null)
                     {
                         party.distanceToLeader = Vector3.Distance(toWorld.Position, position[party.Leader].Position);
                     }
                 })
                 
                 .Schedule(inputDeps);
            
            return DistanceUpdate;
        }
    }

}