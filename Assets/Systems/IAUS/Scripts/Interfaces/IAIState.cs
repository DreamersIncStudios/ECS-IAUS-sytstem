using Unity.Entities;

namespace IAUS.Interfaces
{
    public interface IAIState:IComponentData
    {
        public AIStates Name { get; }
        /// <summary>
        /// AI State score
        /// </summary>
        float TotalScore { get; }
        /// <summary>
        /// Current Operational statues of given AI State
        /// </summary>
        ActionStatus Status { get; set; }
        /// <summary>
        ///  How long should cooldown last.
        /// </summary>
        float CoolDownTime { get; }
        /// <summary>
        /// Is NPC in cooldown;
        /// </summary>
        bool InCooldown { get; }
        /// <summary>
        /// How much time left in oCooldown;
        /// </summary>
        float ResetTime { get; set; }
        float mod { get; }
    }
    
}