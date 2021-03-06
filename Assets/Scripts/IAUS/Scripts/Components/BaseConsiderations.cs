﻿using IAUS.ECS2.Consideration;
namespace IAUS.ECS2
{

    public struct DistanceToConsideration :IBaseConsiderations
    {
        public float Ratio { get; set; }

    }

    /// <summary>
    /// this will be update as part of the health change buffer 
    /// </summary>
    public struct CharacterHealthConsideration : IBaseConsiderations
    {
        public float Ratio { get; set; }

    }
}
