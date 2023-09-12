using AISenses.VisionSystems;
using DreamersInc.InflunceMapSystem;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using PixelCrushers.LoveHate;
using Stats;

namespace Global.Component
{
    [System.Serializable]
    /// Do not add [GenerateAuthoring] tag use AITargetCreate
    public struct AITarget : IComponentData
    {
        public TargetType Type;
        public ClassTitle ClassTitle;
        public uint level;
        public int FactionID;
        public int NumOfEntityTargetingMe;
        [HideInInspector] public int GetInstanceID;
        public bool CanBeTargeted => NumOfEntityTargetingMe < 2;
        [HideInInspector] public int MaxNumberOfTarget; // base off of Threat Level
        public bool CanBeTargetByPlayer;
        public float3 CenterOffset;
        //TODO change to output a relationship level;
      
        public float detectionScore;

    }
    [System.Serializable]
    public enum TargetType
    {
        None, Character, Location, Vehicle
    }



    [UpdateInGroup(typeof(VisionTargetingUpdateGroup))]
    [UpdateBefore(typeof(VisionSystemJobs))]
    public partial class UpdateAITarget : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref AITarget target, ref Perceptibility perceptibility) =>
            {
                target.detectionScore = perceptibility.Score;
            }).Schedule();
        }
    }

}