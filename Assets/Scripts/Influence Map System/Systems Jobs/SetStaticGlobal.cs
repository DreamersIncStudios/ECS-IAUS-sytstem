using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
namespace InfluenceMap
{
    [Unity.Burst.BurstCompile]
    public struct SetStaticGlobal : IJobForEach_B<Gridpoint>
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> StaticEntity;
        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Position;
        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<Influencer> Influence;

        public void Execute(DynamicBuffer<Gridpoint> gridpoints)
        {
            for (int cnt = 0; cnt < gridpoints.Length; cnt++)
            {
                Gridpoint gridSquare = gridpoints[cnt];

                for (int j = 0; j < StaticEntity.Length; j++)
                {
                    float dist = Vector3.Distance(gridSquare.Position, Position[StaticEntity[j]].Position);

                    if (Influence[StaticEntity[j]].influence.Proximity.y > dist) 
                    {
                        gridSquare.Global.Proximity.x = Influence[StaticEntity[j]].influence.Proximity.x / dist;
                    }
                    if (Influence[StaticEntity[j]].influence.Threat.y > dist)
                    {
                        gridSquare.Global.Threat.x = Influence[StaticEntity[j]].influence.Threat.x / dist;
                    }
                }
                gridpoints[cnt] = gridSquare;
            }
        }
    }
}