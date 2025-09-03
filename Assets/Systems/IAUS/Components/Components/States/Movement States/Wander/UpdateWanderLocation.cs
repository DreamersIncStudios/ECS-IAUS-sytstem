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
                    ref UpdateWanderLocationTag tag, ref Movement move, in PackMember packMember) =>
                {
                    Debug.Log("Pack member");

                }).Run();
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