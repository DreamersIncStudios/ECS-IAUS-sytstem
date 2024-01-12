using Components.MovementSystem;
using DreamersInc.QuadrantSystems;
using IAUS.ECS.Systems;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utilities;

namespace IAUS.ECS.Component
{
    public partial class UpdateWanderLocation : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithStructuralChanges().WithoutBurst().ForEach((Entity entity, ref LocalTransform transform, ref WanderQuadrant wander, ref UpdateWanderLocationTag tag, ref Movement move) => {
                if (wander.WanderNeighborQuadrants)
                {
                    List<float3> positions = new List<float3>();

                    positions.Add(GetWanderPoint(transform.Position, wander.HashKey + 1));
                    positions.Add(GetWanderPoint(transform.Position, wander.HashKey - 1));
                    positions.Add(GetWanderPoint(transform.Position, wander.HashKey + NPCQuadrantSystem.quadrantZMultiplier));
                    positions.Add(GetWanderPoint(transform.Position, wander.HashKey - NPCQuadrantSystem.quadrantZMultiplier));
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


        }
        float3 GetWanderPoint(float3 CurPosition, int hashKey)
        {
            while (true) {
                if (GlobalFunctions.RandomPoint(CurPosition, 100, out float3 pos)) {
                    if (NPCQuadrantSystem.GetPositionHashMapKey((int3)pos) == hashKey)
                    {
                        return pos;
                    }
                
                }
            }
        }

    }
}