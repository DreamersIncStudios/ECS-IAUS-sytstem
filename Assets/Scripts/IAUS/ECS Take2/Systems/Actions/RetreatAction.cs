using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.AI;
using IAUS.ECS.Component;
using IAUS.Core;
using IAUS.ECS2.Charaacter;

namespace IAUS.ECS2
{
    [UpdateAfter(typeof(StateScoreSystem))]
    [UpdateInGroup(typeof(IAUS_UpdateState))]

    public class RetreatAction : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle SetRetreatPoint = Entities
                .WithoutBurst()
                .ForEach((ref RetreatTag retreat, ref Movement movement, in NPC npc, in LocalToWorld pos) => {
                    
         



            }).Schedule(inputDeps);

            return SetRetreatPoint;
        }


        bool RandomPoint(Vector3 center, float range, out Vector3 result)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector3 randomPoint = center + Random.insideUnitSphere * range;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }
            result = Vector3.zero;
            return false;
        }
    }
}