using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stats
{   [System.Serializable]
    public class Elemental : BaseStat
    {
        public Elemental()
        {
            BaseValue = 100;
            ExpToLevel = 50;
            LevelModifier = 1.05f;
        }

    }
}
