using InfluenceMap;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Components.MovementSystem;
using Unity.Mathematics;
using IAUS.Core;
using Utilities;
using Utilities.ReactiveSystem;


[assembly: RegisterGenericComponentType(typeof(ReactiveComponentTagSystem<IAUS.ECS2.FollowTargetTag , IAUS.ECS2.FollowCharacter, IAUS.ECS2.FollowTagReactor>.StateComponent))]


namespace IAUS.ECS2
{
    [System.Serializable]
    [GenerateAuthoringComponent]
    public struct FollowCharacter : BaseStateScorer
    {
        public Entity Target;

        public float DistanceToMantainFromTarget;
        public ConsiderationData DistanceToNextPoint;
        // do we need a enemy in range consideration?
        public ConsiderationData Health;

        public float3 TargetLocation;
        public float DistanceAtStart;
        public bool IsTargetMoving;
        public float mod { get { return 1.0f - (1.0f / 2.0f); } }

        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _resetTimer;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;

        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float ResetTimer { get { return _resetTimer; } set { _resetTimer = value; } }
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
    }

    public struct Squad : IComponentData
    {
        public int TeamMemberNumber;
    }
    public struct getpointTag : IComponentData { }

    [UpdateInGroup(typeof(IAUS_UpdateScore))]
    public class FollowStateScore : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float DT = Time.DeltaTime;
            JobHandle CheckLeaderStatus = Entities.ForEach((ref FollowCharacter follow) =>
            {
                
            })
                .Schedule(inputDeps);

            JobHandle FollowScore = Entities.ForEach((ref FollowCharacter follow, in HealthConsideration health) =>
            {
                float targetcheck = follow.IsTargetMoving ? 1.0f : 0.0f;

                float TotalScore = Mathf.Clamp01(follow.Health.Output(health.Ratio)
                    * targetcheck
                    )
                ;
                follow.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * follow.mod) * TotalScore);

            }).Schedule(CheckLeaderStatus);

            return FollowScore;
        }
    }


    [UpdateInGroup(typeof(IAUS_Initialization))]
    [UpdateBefore(typeof(SetupIAUS))]
    public class SetupFollowState : JobComponentSystem
    {

        EntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityCommandBuffer.Concurrent entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
            ComponentDataFromEntity<InfluenceValues> Influences = GetComponentDataFromEntity<InfluenceValues>(true);
            ComponentDataFromEntity<HealthConsideration> health = GetComponentDataFromEntity<HealthConsideration>(true);

            JobHandle SetupFollow = Entities
            .WithNativeDisableParallelForRestriction(health)
            .WithReadOnly(health)
            .WithReadOnly(Influences)
                .WithNativeDisableParallelForRestriction(Influences)
                .ForEach((Entity entity, int nativeThreadIndex, ref DynamicBuffer<StateBuffer> stateBuffer, ref FollowCharacter follow,
                in CreateAIBufferTag c2
               ) =>
                {
                    bool added = true;
                    for (int index = 0; index < stateBuffer.Length; index++)
                    {
                        if (stateBuffer[index].StateName == AIStates.FollowTarget)
                        { added = false; }
                    }



                    if (added)
                    {

                        stateBuffer.Add(new StateBuffer()
                        {
                            StateName = AIStates.FollowTarget,
                            Status = ActionStatus.Idle
                        });
                        if (!health.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<HealthConsideration>(nativeThreadIndex, entity);
                        }
                        if (!Influences.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<InfluenceValues>(nativeThreadIndex, entity);
                        }
                        entityCommandBuffer.AddComponent<getpointTag>(nativeThreadIndex, entity);
                    }
                })
                .Schedule(inputDeps);

            return SetupFollow;
        }
    }


    [UpdateInGroup(typeof(IAUS_UpdateState))]
    [UpdateAfter(typeof(PatrolAction))]
    public class FollowPosition : ComponentSystem
    {

        protected override void OnUpdate()
        {
            ComponentDataFromEntity<Patrol> patrol = GetComponentDataFromEntity<Patrol>(true);

            Entities.ForEach((Entity entity, ref FollowCharacter Follow, ref getpointTag tag) =>
            {
                Vector3 tempPoint = new Vector3();
                float3 Center = patrol[Follow.Target].waypointRef;
                retry:
                if (GlobalFunctions.RandomPoint(Center, 10, out tempPoint))
                {
                    Follow.TargetLocation = tempPoint;
                }
                else { 
                goto retry;
                }
                PostUpdateCommands.RemoveComponent<getpointTag>(entity);
            });
        }
    }
    
    public struct FollowTargetTag : IComponentData
    {
        bool test;
    }

  
    [UpdateInGroup(typeof(IAUS_UpdateState))]
    [UpdateAfter(typeof(PatrolAction))]

    public class FollowAction : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            
            JobHandle Action = Entities.ForEach((ref FollowCharacter follow, ref Movement move, ref InfluenceValues InfluValues,
                in FollowTargetTag tag) =>
            {
                if (follow.Status == ActionStatus.Success)
                    return;


                if (!follow.TargetLocation.Equals(move.TargetLocation)) 
                {
                    move.TargetLocation = follow.TargetLocation;
                    move.SetTargetLocation = true;
                    follow.Status = ActionStatus.Running;
                    move.Completed = false;
                    move.CanMove = true;
                }

                //complete
                if (follow.Status == ActionStatus.Running)
                {
                    if (move.Completed && !move.CanMove)
                    {
                        follow.Status = ActionStatus.Success;
                    }
                }
      

            }
            ).Schedule(inputDeps);

            return Action;
        }
    }
    public struct FollowTagReactor : IComponentReactorTagsForAIStates<FollowTargetTag, FollowCharacter>
    {
        public void ComponentAdded(Entity entity, ref FollowTargetTag newComponent, ref FollowCharacter AIState)
        {
            if (AIState.Status == ActionStatus.Running)
                return;

            AIState.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref FollowCharacter AIState, in FollowTargetTag oldComponent)
        {
            switch (AIState.Status)
            {
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

        public void ComponentValueChanged(Entity entity, ref FollowTargetTag newComponent, ref FollowCharacter AIStateCompoment, in FollowTargetTag oldComponent)
        {
            throw new System.NotImplementedException();
        }

        public class FollowReactiveSystem : ReactiveComponentTagSystem<FollowTargetTag,FollowCharacter, FollowTagReactor>
        {
            protected override FollowTagReactor CreateComponentReactor()
            {
                return new FollowTagReactor();
            }
        }

    }
}


