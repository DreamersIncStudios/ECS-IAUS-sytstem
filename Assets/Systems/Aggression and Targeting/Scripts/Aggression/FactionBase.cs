using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace CharacterAlignmentSystem
{
    public interface FactionBase : IComponentData
    {
        FactionAggression Aggression { get; }
        bool Corrupted { get; set; }
    }
    public struct FactionModifier : IComponentData {
        public FactionAggression AggressionMod;
    }
    public struct Human : FactionBase {
        [SerializeField] public bool Corrupted { get; set; }
        [SerializeField] public FactionAggression Aggression { get; set; }
    }
    public struct Angel : FactionBase {
        [SerializeField] public bool Corrupted { get; set; }
        [SerializeField] public FactionAggression Aggression { get; set; }

    }
    public struct Daemon : FactionBase {
      [SerializeField] public bool Corrupted { get { return true; } set { } }
        [SerializeField] public FactionAggression Aggression { get; set; }

    }
    public struct Mecha : FactionBase {
        [SerializeField] public bool Corrupted { get; set; }
        [SerializeField] public FactionAggression Aggression {get;set;}

    }




}
