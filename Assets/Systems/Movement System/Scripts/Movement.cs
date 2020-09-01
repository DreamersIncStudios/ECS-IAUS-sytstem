
//using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Components.MovementSystem
{
    [System.Serializable]

    [GenerateAuthoringComponent]
    public struct Movement : IComponentData
    {
        public float3 TargetLocation;
        public float MovementSpeed;
        public bool TargetLocationCrowded;
        public int MaxInfluenceAtPoint;


        void SetMovementSpeed(float SpeedStat, float SpeedFactor) // Eqauation needs to be add later to account for Speed Stat or this is done in AI system;
        { MovementSpeed = SpeedFactor; }

        //public float SprintSpeed // To Be Added if needed
        public bool CanMove;
        public bool Completed;
        public float StoppingDistance;
        public float Acceleration;
        public float DistanceRemaining;
        public bool SetTargetLocation { get; set; }
    }



}