using System.Collections.Generic;

namespace IAUS {
    public interface IAction : Prototype<IAction> {
        string NameId { get; }
        List<ConsiderationBase> Considerations { get; set; }
        float TotalScore { get; set; }
        CharacterContext Agent { get; set; }
        ActionStatus actionStatus { get; }
        float Cooldown { get; set; }
        float CooldownTimer { get; }
        void Setup();
        void Score();
        void Execute();
        void OnExit();
        void OnStart();

    }
    public enum ActionStatus {

    Failure,Succes,Running, Interrupted, Idle}
}