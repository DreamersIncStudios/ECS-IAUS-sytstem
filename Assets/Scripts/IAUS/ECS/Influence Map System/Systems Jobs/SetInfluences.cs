using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;

namespace InfluenceMap
{
    public class SetInfluences : JobComponentSystem
    {
        public EntityQueryDesc influencers = new EntityQueryDesc()
        {
            All = new ComponentType[]{ typeof(Influencer),typeof (LocalToWorld)}
        };

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            EntityQuery query = GetEntityQuery(influencers);
            var setinf = new Set()
            {
                InfPos = query.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
                influencers = query.ToComponentDataArray<Influencer>(Allocator.TempJob)
            };
            JobHandle handle = setinf.Schedule(this, inputDeps);
            return handle;
        }
    }

    [BurstCompile]
    public struct Set : IJobForEach_B<Gridpoint>
    {
      [ReadOnly] public NativeArray<LocalToWorld> InfPos;
      [ReadOnly] public NativeArray<Influencer> influencers;

        public void Execute(DynamicBuffer<Gridpoint> gridpoints)
        {
            for(int cnt = 0; cnt< gridpoints.Length;cnt++) {
                Gridpoint temp= gridpoints[cnt];
                temp.TotalValue = 0;
                temp.protection = new Threat();
                temp.threat = new Threat();
                for (int index = 0; index < InfPos.Length; index++) {
               
                    float dist = Vector3.Distance(temp.Position, InfPos[index].Position);
                    float input = ((float)influencers[index].Range - dist) / (float)influencers[index].Range;

                    float ratio = influencers[index].Output(input);

                    temp.TotalValue += influencers[index].Influence * ratio;
                    temp.threat.Global += influencers[index].threat.Global * ratio;
                    temp.threat.Player += influencers[index].threat.Player * ratio;
                    temp.threat.Enemy += influencers[index].threat.Enemy * ratio;

                    temp.protection.Global += influencers[index].Protection.Global * ratio;
                    temp.protection.Player += influencers[index].Protection.Player * ratio;
                    temp.protection.Enemy += influencers[index].Protection.Enemy * ratio;

                }
                gridpoints[cnt] = temp;
            }

        }
    }

}