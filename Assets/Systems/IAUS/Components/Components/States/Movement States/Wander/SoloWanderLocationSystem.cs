using System.Collections.Generic;
using Components.MovementSystem;
using DreamersIncStudio.GAIACollective;
using IAUS.ECS.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IAUS.ECS.Component
{
    [UpdateInGroup(typeof(IAUSUpdateGroup))]
    public partial class SoloWanderLocationSystem : WanderLocationSystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithStructuralChanges()
                .WithoutBurst()
                .WithNone<PackMember>()
                .ForEach((Entity entity,
                    ref LocalToWorld transform,
                    ref WanderActionTag wander,
                    ref Movement move,
                    ref UpdateWanderLocationTag tag) =>
                {
                    Debug.Log("running");
                    ProcessEntityTemplate(entity, ref transform, ref wander, ref move, ref tag);
                })
                .Run();
        }

        protected override float3 ComputeTravelPosition(Entity entity,
            ref LocalToWorld transform,
            ref WanderActionTag wander
        )
        {
            if (false)
            {
                // Choose among neighbor quadrants (outline keeps the original selection behavior)
                var positions = new List<float3>
                {
                    GetWanderPoint(transform.Position, wander.HashKey + 1),
                    GetWanderPoint(transform.Position, wander.HashKey - 1),
                    GetWanderPoint(transform.Position, wander.HashKey)
                };


                return positions[Random.Range(0, 2)];
            }

            return GetWanderPoint(transform.Position, wander.HashKey);
        }
    }
}
