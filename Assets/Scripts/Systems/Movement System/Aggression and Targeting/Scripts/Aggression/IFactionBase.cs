using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace CharacterAlignmentSystem
{
    public interface IFactionBase : IComponentData
    {
        FactionAggression Aggression { get; }
        bool Corrupted { get; set; }
    }
    public struct FactionModifier : IComponentData {
        public FactionAggression AggressionMod;
    }
    public struct Human : IFactionBase {
        [SerializeField] public bool Corrupted { get; set; }
        [SerializeField] public FactionAggression Aggression { get; set; }
    }
    public struct Angel : IFactionBase {
        [SerializeField] public bool Corrupted { get; set; }
        [SerializeField] public FactionAggression Aggression { get; set; }

    }
    public struct Daemon : IFactionBase {
      [SerializeField] public bool Corrupted { get { return true; } set { } }
        [SerializeField] public FactionAggression Aggression { get; set; }

    }
    public struct Mecha : IFactionBase {
        [SerializeField] public bool Corrupted { get; set; }
        [SerializeField] public FactionAggression Aggression {get;set;}

    }




}
