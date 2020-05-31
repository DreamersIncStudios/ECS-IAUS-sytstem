using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.AI;
using Components.MovementSystem;
using IAUS.Core;
using IAUS.ECS2.Character;

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


    }
}