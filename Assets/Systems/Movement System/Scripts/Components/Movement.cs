using Unity.Entities;
using Unity.Mathematics;

namespace Components.MovementSystem
{
    public struct Movement : IComponentData
    {
        public float3 TargetLocation;
        public float MaxMovementSpeed;

        //public float SprintSpeed // To Be Added if needed
        public bool CanMove;

        public float DistanceRemaining;

        public void SetMovementSpeed(float SpeedStat) //TODO Eqauation needs to be add later to account for Speed Stat or this is done in AI system;
        { MaxMovementSpeed = SpeedStat/4.5f; }
        
        public bool SetTargetLocation { get; set; }

        public void SetLocation(float3 position)
        {
            TargetLocation = position;
            SetTargetLocation = true;
            CanMove = true;
        }

    }



}
