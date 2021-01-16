using System.Collections;
using System.Collections.Generic;
using Global.Component;
using IAUS.ECS2.Component;
using IAUS.ECS2;
namespace IAUS.SO.Interfaces
{
    public interface INPCBasics
    {
        AITarget Self{get;}
        List<SOAIState> AIStatesAvailable { get; }

    }
    [System.Serializable]
    public struct SOAIState {
        public AIStates state;
        public IBaseStateScorer stateInfo;
    }
}