using Components.MovementSystem;
using DreamersInc.QuadrantSystems;
using IAUS.ECS.Systems;
using System.Collections;
using System.Collections.Generic;
using DreamersIncStudio.GAIACollective;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

#if  un

#endif

namespace IAUS.ECS.Component
{
    public partial class UpdateWanderLocation : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithStructuralChanges().WithoutBurst().WithNone<PackMember>().ForEach(
                (Entity entity, ref LocalTransform transform, ref WanderQuadrant wander,
                    ref UpdateWanderLocationTag tag, ref Movement move) =>
                {
                    if (wander.WanderNeighborQuadrants)
                    {
                        List<float3> positions = new List<float3>();

                        positions.Add(GetWanderPoint(transform.Position, wander.HashKey + 1));
                        positions.Add(GetWanderPoint(transform.Position, wander.HashKey - 1));
                        positions.Add(GetWanderPoint(transform.Position,
                            wander.HashKey + NPCQuadrantSystem.quadrantZMultiplier));
                        positions.Add(GetWanderPoint(transform.Position,
                            wander.HashKey - NPCQuadrantSystem.quadrantZMultiplier));
                        positions.Add(GetWanderPoint(transform.Position, wander.HashKey));

                        wander.TravelPosition = positions[2];
                    }
                    else
                    {
                        wander.TravelPosition = GetWanderPoint(transform.Position, wander.HashKey);
                    }

                    wander.StartingDistance = Vector3.Distance(wander.TravelPosition, transform.Position);

                    EntityManager.RemoveComponent<UpdateWanderLocationTag>(entity);
                }).Run();
      
            Entities.WithStructuralChanges().WithoutBurst().WithAll<PackMember>().ForEach(
                (Entity entity, ref LocalTransform transform, ref WanderQuadrant wander,
                    ref UpdateWanderLocationTag tag, ref Movement move) =>
                {
                    var pack= EntityManager.GetComponentData<Pack>(entity);
                   
                    var candidates = BuildCandidates(transform.Position, wander.HashKey, wander.WanderNeighborQuadrants);
               

                    // Choose a position that is outside cohesion range if possible; otherwise pick the farthest
                    wander.TravelPosition = ChooseInsideCohesionOrFallback(pack.HerdCenter, candidates, pack.CohesionFactor);

                    // Cache starting distance for movement behavior
                    wander.StartingDistance = Vector3.Distance(wander.TravelPosition, transform.Position);

                    // Clear the tag after updating
                    EntityManager.RemoveComponent<UpdateWanderLocationTag>(entity);

                }).Run();
        }
        
        // Local helpers (combinators)
        float3[] BuildCandidates(float3 origin, int hashKey, bool includeNeighbors)
        {
            if (!includeNeighbors)
            {
                return new[]
                {
                    GetWanderPoint(origin, hashKey)
                };
            }

            // Fixed-size array avoids GC from List allocations
            var arr = new float3[5];
            arr[0] = GetWanderPoint(origin, hashKey + 1);
            arr[1] = GetWanderPoint(origin, hashKey - 1);
            arr[2] = GetWanderPoint(origin, hashKey + NPCQuadrantSystem.quadrantZMultiplier);
            arr[3] = GetWanderPoint(origin, hashKey - NPCQuadrantSystem.quadrantZMultiplier);
            arr[4] = GetWanderPoint(origin, hashKey);
            return arr;
        }
        float3 ChooseInsideCohesionOrFallback(float3 origin, float3[] candidates, float cohesionRadius)
        {
            // Prefer positions INSIDE the cohesion radius (closest wins)
            float bestInsideDist = float.PositiveInfinity;
            int bestInsideIdx = -1;

            // Track absolute closest as deterministic fallback
            float bestDist = float.PositiveInfinity;
            int bestIdx = -1;

            for (int i = 0; i < candidates.Length; i++)
            {
                float d = math.distance(origin, candidates[i]);

                // Prefer positions that are INSIDE the cohesion radius, smallest distance wins
                if (d <= cohesionRadius && d < bestInsideDist)
                {
                    bestInsideDist = d;
                    bestInsideIdx = i;
                }

                // Track absolute closest to have a deterministic fallback
                if (d < bestDist)
                {
                    bestDist = d;
                    bestIdx = i;
                }
            }

            if (bestInsideIdx >= 0) return candidates[bestInsideIdx];
            return candidates[bestIdx >= 0 ? bestIdx : 0];
        }

        const float WanderRange = 50f;
        float3 GetWanderPoint(float3 currentPosition, int hashKey)
        {
            var x = 0;
            while (x< 50)
            {
                if (!GlobalFunctions.RandomPoint(currentPosition, WanderRange, out float3 potentialPosition)) continue;
                if (!IsPositionValid((int3)potentialPosition, hashKey)) continue;
                if (IsPathComplete(currentPosition, potentialPosition))
                    return potentialPosition;
                else
                {
                    x++;
                }
            }
            Debug.Log("Can't find position");
            return currentPosition;
        }
        
        bool IsPositionValid(int3 position, int hashKey)
        {
            return NPCQuadrantSystem.GetPositionHashMapKey(position) == hashKey;
        }

        bool IsPathComplete(float3 start, float3 end)
        {
            NavMeshPath path = new NavMeshPath();
            return NavMesh.CalculatePath(start, end, 1<<NavMesh.GetAreaFromName("Walkable"), path) &&
                   path.status == NavMeshPathStatus.PathComplete;
        }
    }
}