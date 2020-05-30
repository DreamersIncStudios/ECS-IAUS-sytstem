using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Components.MovementSystem;
using IAUS.Core;
using IAUS.ECS2.Character;

namespace IAUS.ECS2
{
    [UpdateAfter(typeof(StateScoreSystem))]
    [UpdateInGroup(typeof(IAUS_UpdateState))]

    public class ReturnToBaseAction : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ComponentDataFromEntity<LocalToWorld> position = GetComponentDataFromEntity<LocalToWorld>();

            JobHandle ReturnActionJob = Entities
                .WithNativeDisableParallelForRestriction(position)
                .WithReadOnly(position)
                .ForEach((ref ReturnToBase returnToBase, ref Movement movement, in NPC npc) => 
            {

                    movement.TargetLocation = position[npc.HomeEntity].Position;

            }).Schedule(inputDeps);

            return ReturnActionJob;
        }
    }
}