using Components.MovementSystem;
using DreamersInc.QuadrantSystems;
using IAUS.ECS.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using Utilities;


namespace IAUS.ECS.Component
{ // Base system with the template-method skeleton for processing an entity update
    public abstract partial class WanderLocationSystemBase : SystemBase
    {
        protected const float WanderRange = 50f;

        protected override void OnUpdate()
        {
            // Intentionally empty: concrete systems execute their own entity queries
            // and invoke ProcessEntityTemplate(...) for each matched entity.
        }

    
        protected void ProcessEntityTemplate(Entity entity,
                                             ref LocalTransform transform,
                                             ref WanderQuadrant wander,
                                             ref UpdateWanderLocationTag tag,
                                             ref Movement move)
        {
            float3 travel = ComputeTravelPosition(entity, ref transform, ref wander, ref move);
            wander.TravelPosition = travel;

            // Shared post-steps
            wander.StartingDistance = Vector3.Distance(wander.TravelPosition, transform.Position);
            EntityManager.RemoveComponent<UpdateWanderLocationTag>(entity);
        }

        // Variation point implemented by subclasses
        protected abstract float3 ComputeTravelPosition(Entity entity,
                                                        ref LocalTransform transform,
                                                        ref WanderQuadrant wander,
                                                        ref Movement move);

        //  helpers
        protected float3 GetWanderPoint(float3 currentPosition, int hashKey)
        {
            var attempts = 0;
            while (attempts < 50)
            {
                if (!GlobalFunctions.RandomPoint(currentPosition, WanderRange, out float3 potentialPosition) || !IsPositionValid((int3)potentialPosition, hashKey))
                {
                    attempts++;
                    continue;
                }

                if (IsPathComplete(currentPosition, potentialPosition))
                    return potentialPosition;

                attempts++;
            }

            Debug.Log("Can't find position");
            return currentPosition;
        }

        protected bool IsPositionValid(int3 position, int hashKey)
        {
            return NPCQuadrantSystem.GetPositionHashMapKey(position) == hashKey;
        }

        protected bool IsPathComplete(float3 start, float3 end)
        {
            var path = new NavMeshPath();
            return NavMesh.CalculatePath(start, end, 1 << NavMesh.GetAreaFromName("Walkable"), path) &&
                   path.status == NavMeshPathStatus.PathComplete;
        }
    }
}