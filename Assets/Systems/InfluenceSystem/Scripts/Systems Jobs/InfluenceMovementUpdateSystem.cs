using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Components.MovementSystem;
// move to  separate folder and create new assembly defination 
namespace InfluenceMap
{
    public class InfluenceMovementUpdateSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle Update = Entities.ForEach((ref Movement move, ref InfluenceValues InfluValues) =>
             {



                 //Too many people that this locaation
                 if (move.DistanceRemaining <= 10.5f && move.DistanceRemaining >= 3.5)
                 {
                     if (InfluValues.InfluenceAtTarget.Ally.Proximity.x > move.MaxInfluenceAtPoint)
                     {
                         move.TargetLocationCrowded = true;
                     }

                    }

             }
        ).Schedule(inputDeps);
            return Update;
        } 
    }
}