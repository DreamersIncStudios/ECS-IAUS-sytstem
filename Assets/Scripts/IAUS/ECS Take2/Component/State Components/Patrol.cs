using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;
using Utilities.ReactiveSystem;
using System;
using ProjectRebirth.Bestiary.Interfaces;

[assembly: RegisterGenericComponentType(typeof(ReactiveComponentTagSystem<IAUS.ECS2.PatrolActionTag, IAUS.ECS2.Patrol, IAUS.ECS2.PatrolTagReactor>.StateComponent))]

namespace IAUS.ECS2
{
    [RequireComponentTag(typeof(PatrolBuffer))]

    //PatrolAction requires WaitAction just set wait to lowValue;
    [GenerateAuthoringComponent]
    public struct Patrol : BaseStateScorer
    {
        public ConsiderationData Health;
        public ConsiderationData DistanceToTarget;
        [HideInInspector] public bool UpdatePatrolPoints;
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _resetTimer;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;
        public bool UpdatePostition;
        public float DistanceAtStart;
        public int index;
        public float mod { get { return 1.0f - (1.0f / 2.0f); } }
        public float BufferZone;
        //public int MaxInfluenceAtPoint;
        public float DistInfluence;
        public bool LeaderUpdate;
        public float3 waypointRef;
        public int MaxNumWayPoint{ get;set; }
        public Entity HomeEntity { get; set; }
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }

       
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float ResetTimer { get { return _resetTimer; } set { _resetTimer = value; } }
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

        public bool CanPatrol;
    }
    public struct PatrolActionTag : IComponentData {
        public bool test;

    }
    public struct PatrolTagReactor : IComponentReactorTags<PatrolActionTag,Patrol>
    {
        public void ComponentAdded(Entity entity,ref PatrolActionTag newComponent, ref Patrol AIState)
        {
            if (AIState.Status == ActionStatus.Running)
                return;

            AIState.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref Patrol AIState, in PatrolActionTag oldComponent)
        {
       
            if (!AIState.CanPatrol)
            {
                AIState.Status = ActionStatus.Disabled;
                // patrol.TotalScore = 0.0f;
               
            }

            switch (AIState.Status) {
                case ActionStatus.Running:
                    break;
                case ActionStatus.Interrupted:
                    AIState.ResetTime = AIState.ResetTimer / 2.0f;
                    AIState.Status = ActionStatus.CoolDown;
                    break;
                case ActionStatus.Success:
                    AIState.ResetTime = AIState.ResetTimer;
                    AIState.Status = ActionStatus.CoolDown;
                    break;
                case ActionStatus.Failure:
                    AIState.ResetTime = AIState.ResetTimer / 2.0f;
                    AIState.Status = ActionStatus.CoolDown;
                    break;
                case ActionStatus.Disabled:
                    AIState.TotalScore = 0.0f;
                    break;
            }
        }

        public void ComponentValueChanged(Entity entity, ref Patrol AIState, ref PatrolActionTag  newComponent, in PatrolActionTag oldComponent)
        {
            Debug.Log("Changed");
      //      newComponent.test = false; ;

        }

        public class PatrolReactiveSystem : ReactiveComponentTagSystem<PatrolActionTag,Patrol, PatrolTagReactor>
        {
            protected override PatrolTagReactor CreateComponentReactor()
            {
                return new PatrolTagReactor();
            }


        }
    }
}