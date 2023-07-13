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
                    List<float3> positions = new List<float3>
                    {
                        GetWanderPoint(transform.Position, wander.Hashkey + 1),
                        GetWanderPoint(transform.Position, wander.Hashkey - 1),
                        GetWanderPoint(transform.Position, wander.Hashkey + NPCQuadrantSystem.quadrantYMultiplier),
                        GetWanderPoint(transform.Position, wander.Hashkey - NPCQuadrantSystem.quadrantYMultiplier),
                        GetWanderPoint(transform.Position, wander.Hashkey + 1 + NPCQuadrantSystem.quadrantYMultiplier),
                        GetWanderPoint(transform.Position, wander.Hashkey -1 + NPCQuadrantSystem.quadrantYMultiplier),
                        GetWanderPoint(transform.Position, wander.Hashkey + 1 - NPCQuadrantSystem.quadrantYMultiplier),
                        GetWanderPoint(transform.Position, wander.Hashkey -1 - NPCQuadrantSystem.quadrantYMultiplier),
                        GetWanderPoint(transform.Position, wander.Hashkey)
                    };

                    int index = UnityEngine.Random.Range(0, positions.Count);
                    wander.TravelPosition = positions[index];
                }
                else
                {
                   wander.TravelPosition=GetWanderPoint(transform.Position, wander.Hashkey);
                }

                wander.StartingDistance = Vector3.Distance(wander.TravelPosition, transform.Position);

                EntityManager.RemoveComponent<UpdateWanderLocationTag>(entity);
            }).Run();


        }
        float3 GetWanderPoint(float3 CurPosition, int hashKey)
        {
            float3 position = new float3();
            while (position.Equals( float3.zero)) {
                if (GlobalFunctions.RandomPoint(CurPosition, 150, out float3 pos)) {
                    if (NPCQuadrantSystem.GetPositionHashMapKey((int3)pos) == hashKey) 
                    {
                        position = pos; 
                        break;
                    }
                
                }
            }
            return position;
        }

    }
}