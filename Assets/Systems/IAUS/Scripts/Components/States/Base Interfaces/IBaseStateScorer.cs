using Unity.Entities;

namespace IAUS.ECS.Component {

    public interface IBaseStateScorer : IComponentData
    {
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
        /// How much time left in oooldown;
        /// </summary>
        float ResetTime { get; set; }
        float mod { get; }
       // public int refIndex { get; set; }

    }

    public enum ActionStatus   
    {
        Idle, Success, Running, Interrupted, CoolDown, Disabled, Failure
    }
}
