
//using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Components.MovementSystem
{
    [System.Serializable]
    [GenerateAuthoringComponent]
    public struct Movement : IComponentData
    {
        public float3 TargetLocation { get; private set; }
        public float MovementSpeed;
        public bool TargetLocationCrowded;
        public int MaxInfluenceAtPoint;
        public float3 Offset;
        public bool WithinRangeOfTargetLocation => DistanceRemaining <= StoppingDistance;

        void SetMovementSpeed(float SpeedStat, float SpeedFactor) // Eqauation needs to be add later to account for Speed Stat or this is done in AI system;
        { MovementSpeed = SpeedFactor; }

        //public float SprintSpeed // To Be Added if needed
        public bool CanMove { get; set; }
        public bool Completed => WithinRangeOfTargetLocation ;
        public float StoppingDistance;
        public float Acceleration;
        public float DistanceRemaining;
        public bool SetTargetLocation { get; set; }
        public void SetLocation(float3 position) {
            TargetLocation = position;
            SetTargetLocation = true;
            CanMove = true;
        }
    }



}