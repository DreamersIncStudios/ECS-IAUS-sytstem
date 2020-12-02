using Unity.Entities;

namespace IAUS.ECS2.Component {

    public interface IBaseStateScorer : IComponentData
    {
        float TotalScore { get; set; }
        ActionStatus Status { get; set; }
        float ResetTimer { get; set; }
        float ResetTime { get; set; }
        float mod { get; }
    }

    public enum ActionStatus   
    {
         Success, Running, Interrupted, Idle, CoolDown, Disabled, Failure
    }
}
