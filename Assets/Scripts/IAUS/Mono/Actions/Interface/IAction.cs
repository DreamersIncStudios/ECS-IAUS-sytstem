using System.Collections.Generic;

namespace IAUS {
    public interface IAction : Prototype<IAction> {
        string NameId { get; }
        List<ConsiderationBase> Considerations { get; set; }
        float TotalScore { get; set; }
        CharacterContext Agent { get; set; }
        ActionStatus ActionStatus { get; }
        float Cooldown { get; set; }
        float ElapsedTime { get; }
        void Setup();
        void Score();
        void Excute();
        void OnExit();
        void OnStart();

    }
    public enum ActionStatus {

    Failure,Succes,Running, Idle, InCoolDown}
}