using AISenses.VisionSystems;
using DreamersInc.InflunceMapSystem;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using PixelCrushers.LoveHate;

namespace Global.Component
{
    [System.Serializable]
    /// Do not add [GenerateAuthoring] tag use AITargetCreate
    public struct AITarget : IComponentData
    {
        public TargetType Type;
        public int FactionID;
        public int NumOfEntityTargetingMe;
        [HideInInspector] public int GetInstanceID;
        public bool CanBeTargeted => NumOfEntityTargetingMe < 2;
        [HideInInspector] public int MaxNumberOfTarget; // base off of Threat Level
        public bool CanBeTargetByPlayer;
        public float3 CenterOffset;
        //TODO change to output a relationship level;
        public bool IsFriend(int factionID)
        {
            bool test = new bool();
            if (factionID == FactionID)
                test = true;
            else
            {
                test = LoveHate.factionDatabase.GetFaction(factionID).GetPersonalAffinity(FactionID) > 51;
            }
            return test;
        }
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