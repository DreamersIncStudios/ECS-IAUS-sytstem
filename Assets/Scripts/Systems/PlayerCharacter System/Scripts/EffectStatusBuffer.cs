using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Stats
{
    public struct EffectStatusBuffer : IBufferElementData
    {
        public StatusEffect Status;

        public static implicit operator StatusEffect(EffectStatusBuffer e) { return e.Status; }
        public static implicit operator EffectStatusBuffer(StatusEffect e) { return new EffectStatusBuffer { Status = e }; }
    }
}


        