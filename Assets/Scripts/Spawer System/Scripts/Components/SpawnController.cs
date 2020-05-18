using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpawnerSystem
{
    public class SpawnController : MonoBehaviour
    {
        [HideInInspector]public static SpawnController Instance;
        public bool CanSpawn { get { return CountInScene < MaxCountInScene; } }
        public uint CountInScene;
        public uint MaxCountInScene;
        public SpawnControlMode ControlMode;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
                Destroy(this);

        }
        
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    public enum SpawnControlMode {
        Game,
        WaveGen
    }
}