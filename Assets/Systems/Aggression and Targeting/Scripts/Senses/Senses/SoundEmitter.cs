using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
namespace AISenses.HearingSystem
{
    [GenerateAuthoringComponent]
    public struct SoundEmitter : IComponentData
    {
        public int SoundLevel;
        public SoundType Sound;
    }
    public enum SoundType
    {
        Ambient,
        Alarm,
        Suspicious

    }
}
