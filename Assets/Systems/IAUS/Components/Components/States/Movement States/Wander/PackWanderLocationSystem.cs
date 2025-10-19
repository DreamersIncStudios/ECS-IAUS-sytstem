using Components.MovementSystem;
using DreamersIncStudio.GAIACollective;
using IAUS.ECS.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace IAUS.ECS.Component
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class PackWanderLocationSystem : WanderLocationSystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithStructuralChanges()
                .WithoutBurst()
                .WithAll<PackMember>()
                .ForEach((Entity entity,
                    ref LocalToWorld transform,
                    ref WanderActionTag wander,
                    ref Movement move,
                    ref UpdateWanderLocationTag tag
                   ) =>
                {
                    // Maintain herd center update behavior
       

                    ProcessEntityTemplate(entity, ref transform, ref wander, ref move, ref tag);
                })
                .Run();
        }

        protected override float3 ComputeTravelPosition(Entity entity,
            ref LocalToWorld transform,
            ref WanderActionTag wander)
        {
            // Minimal outline: reuse current hash-based wander point.
            // You could refine this to bias toward wander.WanderCenterPoint if desired.
            return GetWanderPoint(transform.Position, wander.HashKey);
        }
    }
}