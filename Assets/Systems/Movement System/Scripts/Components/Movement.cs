using Unity.Entities;
using Unity.Mathematics;

namespace Components.MovementSystem
{
    public struct Movement : IComponentData
    {
        public float3 TargetLocation;
        public float MaxMovementSpeed { get; private set; }
        public bool TargetLocationCrowded;
        public int MaxInfluenceAtPoint;
        public float3 Offset;
        public bool WithinRangeOfTargetLocation => DistanceRemaining <= StoppingDistance;

        public void SetMovementSpeed(float SpeedStat) //TODO Eqauation needs to be add later to account for Speed Stat or this is done in AI system;
        { MaxMovementSpeed = 4.5f; } //TODO set based on stats 

        //public float SprintSpeed // To Be Added if needed
        public bool CanMove;
        [UnityEngine.SerializeField] public bool Completed => WithinRangeOfTargetLocation;
        public float StoppingDistance;
        public float Acceleration;
        public float DistanceRemaining;
        public bool SetTargetLocation { get; set; }

        public void SetLocation(float3 position)
        {
            TargetLocation = position;
            SetTargetLocation = true;
            CanMove = true;
        }

    }



}