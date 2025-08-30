using DreamersIncStudio.GAIACollective;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

using static Unity.Entities.SystemAPI;

namespace DreamersIncStudio.GAIACollective
{
    [UpdateInGroup(typeof(GaiaUpdateGroup))]
    [UpdateAfter(typeof(GaiaSpawnSystem))]
    public partial struct GaiaPackMovement : ISystem
    {
    
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (pack, transform) in Query<Pack, RefRW<LocalTransform>>())
            {
                var cohesion = float3.zero;
                var alignment = float3.zero;
                var separation = float3.zero;
                foreach (var memberTransform in Query<LocalTransform>().WithAll<PackMember>())
                {
                    if (memberTransform.Position.Equals(transform.ValueRO.Position)) continue;
                    float3 directionToMember = memberTransform.Position - transform.ValueRO.Position;
                    float distance = math.length(directionToMember);

                    // Cohesion: Move towards center point
                    cohesion += memberTransform.Position;

                    // Separation: Avoid other members
                    if (distance < pack.SeparationFactor)
                        separation -= directionToMember / distance;

                    // Alignment: Match direction
                    alignment += memberTransform.Forward();
                }

                // Final adjustments
                cohesion /= pack.MemberCount;
                alignment = math.normalize(alignment);

                // Update position & direction
                float3 moveDirection = cohesion + alignment + separation;
                moveDirection = math.normalize(moveDirection);
                transform.ValueRW.Position += moveDirection * SystemAPI.Time.DeltaTime;

            }
        }
    }
}