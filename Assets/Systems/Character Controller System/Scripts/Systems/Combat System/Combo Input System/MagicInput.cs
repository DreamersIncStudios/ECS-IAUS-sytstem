using Unity.Entities;



namespace DreamersInc.ComboSystem
{
   public struct AnimationSpeedMod : IComponentData
    {
        public float SpeedValue;
        public float MaxDuration;
    }
    
}