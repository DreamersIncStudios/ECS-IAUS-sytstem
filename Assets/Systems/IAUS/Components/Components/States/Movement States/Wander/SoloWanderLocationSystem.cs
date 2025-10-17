using System.Collections.Generic;
using Components.MovementSystem;
using DreamersIncStudio.GAIACollective;
using IAUS.ECS.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
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
                    ref WanderQuadrant wander,
                    ref UpdateWanderLocationTag tag) =>
                {
                    ProcessEntityTemplate(entity, ref transform, ref wander, ref tag);
                })
                .Run();
        }

        protected override float3 ComputeTravelPosition(Entity entity,
            ref LocalToWorld transform,
            ref WanderQuadrant wander
        )
        {
            if (wander.WanderNeighborQuadrants)
            {
                // Choose among neighbor quadrants (outline keeps original selection behavior)
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
