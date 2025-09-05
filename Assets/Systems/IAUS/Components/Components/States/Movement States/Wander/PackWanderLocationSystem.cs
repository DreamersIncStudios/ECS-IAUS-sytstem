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
                    ref LocalTransform transform,
                    ref WanderQuadrant wander,
                    ref UpdateWanderLocationTag tag,
                    ref Movement move,
                    in PackMember packMember) =>
                {
                    // Maintain herd center update behavior
                    wander.WanderCenterPoint = EntityManager.GetComponentData<Pack>(packMember.PackEntity).HerdCenter;

                    ProcessEntityTemplate(entity, ref transform, ref wander, ref tag, ref move);
                })
                .Run();
        }

        protected override float3 ComputeTravelPosition(Entity entity,
            ref LocalTransform transform,
            ref WanderQuadrant wander,
            ref Movement move)
        {
            // Minimal outline: reuse current hash-based wander point.
            // You could refine this to bias toward wander.WanderCenterPoint if desired.
            return GetWanderPoint(transform.Position, wander.HashKey);
        }
    }
}