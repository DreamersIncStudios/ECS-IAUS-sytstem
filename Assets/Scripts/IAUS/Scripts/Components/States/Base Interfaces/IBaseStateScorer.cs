using Unity.Entities;

namespace IAUS.ECS2.Component {

    public interface IBaseStateScorer : IComponentData
    {
        float TotalScore { get; set; }
        ActionStatus Status { get; set; }
        float CoolDownTime { get; }
        bool InCooldown { get; }
        float ResetTime { get; set; }
        float mod { get; }
    }

    public enum ActionStatus   
    {
        Idle, Success, Running, Interrupted, CoolDown, Disabled, Failure
    }
}
