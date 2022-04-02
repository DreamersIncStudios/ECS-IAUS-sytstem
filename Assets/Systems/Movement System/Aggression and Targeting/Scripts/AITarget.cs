using Unity.Entities;
using UnityEngine;
using DreamersInc.InflunceMapSystem;
using PixelCrushers.LoveHate;
using Unity.Mathematics;

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
        public bool IsFriend(int factionID) {
            bool test = new bool();
            if (factionID == FactionID)
                test = true;
            else {
                test = LoveHate.factionDatabase.GetFaction(factionID).GetPersonalAffinity(FactionID) > 51;
                    }
            return test; }
        public  float detectionScore;

    }
    public partial class UpdateAITarget : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithBurst().ForEach((ref AITarget target, in Perceptibility perceptibility) => {
                target.detectionScore = perceptibility.Score;
            }).ScheduleParallel();
        }
    }


    [System.Serializable]
    public enum TargetType
    {
        None, Character, Location, Vehicle
    }


}