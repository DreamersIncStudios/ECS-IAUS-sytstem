using System.Collections.Generic;

namespace IAUS {
    public interface IAction : Prototype<IAction> {
        string NameId { get; }
        List<ConsiderationBase> Considerations { get; set; }
        float TotalScore { get; set; }
        CharacterContext Agent { get; set; }
        void Setup();
        void Score();
        void OnUpdate();
        void OnExit();
        void OnStart();

    }

}