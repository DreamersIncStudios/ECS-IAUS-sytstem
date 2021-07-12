using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using AISenses;
namespace AISenses.HearingSystem
{
    [GenerateAuthoringComponent]
    public struct SoundEmitter : IComponentData
    {
        public int SoundLevel;
        public SoundTypes Sound;
    }
 }

