﻿using AISenses.VisionSystems;
using DreamersInc.InflunceMapSystem;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Global.Component
{
    [System.Serializable]
    /// Do not add [GenerateAuthoring] tag use AITargetCreate
    public struct AITarget : IComponentData
    {
        public TargetType Type;
        public Race GetRace;
        public int FactionID;

        public int NumOfEntityTargetingMe;
        [HideInInspector] public int GetInstanceID;
        public bool CanBeTargeted => NumOfEntityTargetingMe < 2;
        [HideInInspector] public int MaxNumberOfTarget; // base off of Threat Level
        public bool CanBeTargetByPlayer;
        public float3 CenterOffset;
        public float detectionScore;

        public bool IsFriend(Race race)
        {
            bool test = new bool();
            switch (race)
            {
                case Race.Angel:
                    switch (GetRace)
                    {
                        case Race.Angel:
                        case Race.Human:
                            test = true;
                            break;
                        case Race.Daemon:
                            test = false;
                            break;
                    }
                    break;
                case Race.Daemon:
                    switch (GetRace)
                    {
                        case Race.Angel:
                        case Race.Human:
                            test = false;
                            break;
                        case Race.Daemon:
                            test = true;
                            break;
                    }
                    break;
                case Race.Human:
                    switch (GetRace)
                    {
                        case Race.Angel:
                        case Race.Human:
                            test = true;
                            break;
                        case Race.Daemon:
                            test = false;
                            break;
                    }
                    break;
            }

            return test;
        }


    }
    [System.Serializable]
    public enum TargetType
    {
        None, Character, Location, Vehicle
    }

    //replace with threat score system at later date

    public enum Race
    {
        None, Angel, Daemon, Human // More Types of be added 

    }
    [UpdateInGroup(typeof(VisionTargetingUpdateGroup))]
    [UpdateBefore(typeof(VisionSystemJobs))]
    public partial class UpdateAITarget : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref AITarget target, ref Perceptibility perceptibility) =>
            {
                target.detectionScore = perceptibility.Score;
            });
        }
    }

}