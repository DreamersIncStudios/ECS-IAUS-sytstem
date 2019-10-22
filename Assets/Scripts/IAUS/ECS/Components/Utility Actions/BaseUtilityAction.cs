using Unity.Entities;

namespace IAUS.ECS.Component { 
    public interface BaseUtilityAction : IComponentData
    {
        float Score { get; set; }
        States state { get; set; }
        float Cooldown { get; set; }
        float Timer { get; set; }
        bool InCooldown();
    }
    public enum States {
        Idle,
        Running,
        Completed,
        Interrupted
    }
}