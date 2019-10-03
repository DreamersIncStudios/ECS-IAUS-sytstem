using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS.Consideration;
using Unity.Jobs;
using IAUS.ECS.ComponentStates;

namespace IAUS.ECS.ComponentStates
{

    public class WaitFor : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float TimeAtLoc;
        public float MinRange = 3.0f;
        public float MaxRange = 75.0f;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new WaitForComponent() { Timer =0.0f, TimeAtLoc = TimeAtLoc,
                MaxRange = MaxRange,
                MinRange = MinRange,
                WaitForConsider= new ConsiderationBaseECS
                {
                    responseType = ResponseTypeECS.Logistic,
                    M = 50,
                    K = .95f,
                    B = 0.05f,
                    C = 0.6f
                },
                DistanceConsider = new ConsiderationBaseECS {
                    responseType = ResponseTypeECS.Logistic,
                    M = 50,
                    K = -.95f,
                    B = 1,
                    C = .6f
                }
            };
            dstManager.AddComponentData(entity, data);
        }

    }


    public struct WaitForComponent : IComponentData {
        public float Score;
        public float TimeAtLoc;
        public float Timer;
        public ConsiderationBaseECS DistanceConsider;
        public ConsiderationBaseECS WaitForConsider;
        public float MaxRange;
        public float MinRange;
        public float input(float dist)
        {
            return Mathf.Clamp01((float)(dist - MinRange) / (float)(MaxRange - MinRange));
        }

    }
}
namespace IAUS.ECS.Jobs { 
  
    [UpdateAfter(typeof(MoveScoreJob))]
    public class WaitForScoreJob : JobComponentSystem
    {
        struct Consider : IJobForEach<WaitForComponent>
        {
            public float DT;
            public void Execute(ref WaitForComponent Wait)
            {
                if (Wait.Timer >= 0.0f) {
                    Wait.Timer -= DT;
                }
                if (Wait.Timer < 0.0f)
                    Wait.Timer = 0.0f;
                        

            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new Consider() { DT=Time.deltaTime};
            return job.Schedule(this, inputDeps);
        }
    }
}