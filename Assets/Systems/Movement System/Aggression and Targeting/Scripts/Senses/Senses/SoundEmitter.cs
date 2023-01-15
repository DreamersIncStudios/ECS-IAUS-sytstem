using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using AISenses;
namespace AISenses
{
    [GenerateAuthoringComponent]
    public struct SoundEmitter : IComponentData
    {
        public int SoundLevel;
        public SoundTypes Sound;
        public float LifeTime;
        public bool DestroyThis => LifeTime <= 0.0f;
    }

    public enum SoundLife { 
        PlayOnce, Loop, LoopWhile, LoopAfter
    }
 }

