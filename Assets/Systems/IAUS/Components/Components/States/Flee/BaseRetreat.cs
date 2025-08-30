using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using System.Collections.Generic;
using DreamersInc.InfluenceMapSystem;
using Unity.Burst;
using IAUS.ECS.Consideration;
using System;
namespace IAUS.ECS.Component
{

    public interface BaseRetreat : IBaseStateScorer
    {
        //Need to add a check to see if escape is possible
        ConsiderationScoringData HealthRatio { get; }
        ConsiderationScoringData ProximityInArea { get; }
        ConsiderationScoringData ThreatInArea { get; }
        int FactionMemberID { get; set; }
        float3 LocationOfHighestThreat { get; }
        float3 LocationOfLowestThreat { get; }
        float2 InfluenceValueAtPos { get; }

    }



    
    public struct RetreatActionTag : IComponentData { readonly bool test;
    }

   
}