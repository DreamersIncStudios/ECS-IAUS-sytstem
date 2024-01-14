using System.Collections.Generic;
using UnityEngine;

namespace DreamersInc.ComboSystem
{
    public interface ICombos
    {
       // List<AnimationCombo> ComboList { get; }

        void UnlockCombo(ComboNames Name);
        void DisplayCombo();

    }
}