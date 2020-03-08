using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;


namespace IAUS.ECS2
{
    [GenerateAuthoringComponent]
    public struct InvestigateArea : BaseStateScorer
    {
        #region Considerations
        public ConsiderationData Health;
        public ConsiderationData DistanceToTarget;
        public ConsiderationData DetectionLevel;
        #endregion


        #region BaseState
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _resetTimer;
        [SerializeField] float _resetTime;

        [SerializeField] float _totalScore;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }


        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float ResetTimer { get { return _resetTimer; } set { _resetTimer = value; } }
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        #endregion
    }

    public struct InvestigateAreaTag : IComponentData { }


    //public class InvestigateAreaAction : JobComponentSystem
    //{
    //    protected override JobHandle OnUpdate(JobHandle inputDeps)
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //}
    [Unity.Burst.BurstCompile]
    public struct InvestigateAreaAdd : IJobForEachWithEntity_EBCC<StateBuffer, InvestigateArea, CreateAIBufferTag>
    {

        [ReadOnly] [NativeDisableParallelForRestriction] public ComponentDataFromEntity<HealthConsideration> health;
        [ReadOnly] [NativeDisableParallelForRestriction] public ComponentDataFromEntity<Detection> Detect;

        [NativeDisableParallelForRestriction] public EntityCommandBuffer entityCommandBuffer;

        public void Execute(Entity entity, int Tindex, DynamicBuffer<StateBuffer> stateBuffer, [ReadOnly] ref InvestigateArea c1, [ReadOnly]ref CreateAIBufferTag c2)
        {
            bool add = true;
            for (int index = 0; index < stateBuffer.Length; index++)
            {
                if (stateBuffer[index].StateName == AIStates.InvestigateArea)
                { add = false; }
            }

            if (add)
            {
                stateBuffer.Add(new StateBuffer()
                {
                    StateName = AIStates.InvestigateArea,
                    Status = ActionStatus.Idle
                });
                if (!health.Exists(entity))
                {
                    entityCommandBuffer.AddComponent<HealthConsideration>(entity);

                }
                if (!Detect.Exists(entity))
                {
                    entityCommandBuffer.AddComponent<Detection>(entity);
                    throw new System.Exception(" this does not have Detection component attached to game object. Please attach detection in editor and set default value in order to use");
                }
            }
        }
    }
    
}