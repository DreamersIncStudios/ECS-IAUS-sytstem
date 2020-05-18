using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpawnerSystem.ScriptableObjects;

namespace SpawnerSystem
{
    public class SpawnableSO : ScriptableObject, ISpawnable,IObject
    {
        [SerializeField] int _spawnID;
        [SerializeField] Vector3 _spawnOffset;
        [SerializeField] GameObject _GO;
        [SerializeField] Vector3 _scale;
        public int SpawnID { get { return _spawnID; } set { _spawnID = value; } }
        public Vector3 SpawnOffset { get { return _spawnOffset; } }
        public GameObject GO { get { return _GO; } }
        public Vector3 Scale { get { return _scale; } }
    }
}