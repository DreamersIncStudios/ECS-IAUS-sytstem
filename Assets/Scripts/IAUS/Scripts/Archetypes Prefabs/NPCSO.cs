using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.SO.Interfaces;
using Global.Component;
using IAUS.ECS2.Component;
namespace IAUS.SO
{
    public class NPCSO : ScriptableObject, INPCBasics
    {
        [SerializeField] AITarget _self;
        public AITarget Self { get { return _self; } }
        public List<SOAIState> AIStatesAvailable { get { return states; } }
            [SerializeField] List<SOAIState> states;

        public void Setup(AITarget self, List<SOAIState> stateScorers) {
            _self = self;
            states = stateScorers;
            Debug.Log(states.Count);
        }
        public void Spawn() { }
    }
}
