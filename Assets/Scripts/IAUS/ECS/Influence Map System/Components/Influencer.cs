using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace InfluenceMap
{
    [GenerateAuthoringComponent]
    public struct Influencer : IComponentData
    {
        public float Influence;
        public Threat threat;
        public Threat Protection;
        public int Range;
        public FallOff fallOff;
    }
    [System.Serializable]
    public struct Threat {
        public float Global;
        public float Player, Enemy ;// To Be Expanded later
    }
    public enum FallOff {
        linear,
        Quadratic,
        Inverse,
        Ring,
        Barrier
           
            S

    }



}