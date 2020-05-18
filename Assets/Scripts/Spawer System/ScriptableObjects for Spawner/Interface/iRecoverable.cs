using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpawnerSystem.ScriptableObjects
{
    public interface iRecoverable
    {
        RecoverType recoverType { get; }
        int RecoverAmount { get; }

    }
}