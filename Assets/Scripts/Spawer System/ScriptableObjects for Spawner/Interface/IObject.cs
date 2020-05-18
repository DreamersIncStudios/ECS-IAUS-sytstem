using UnityEngine;

namespace SpawnerSystem.ScriptableObjects
{
    public interface ISpawnable {
        int SpawnID { get; set; }
        Vector3 SpawnOffset { get; }

    }

    public interface IObject 
    {
        GameObject GO { get; }
        Vector3 Scale { get;}
    }
}