using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpawnerSystem
{
    [ExecuteInEditMode]
    public class Spawner : MonoBehaviour
    {
        public uint SpawnPointID;
        public bool Temporoary;
        public List<int> SpawnIDList;


        private void Awake()
        {
            AssignNumber();
        }
       bool CheckID
        {
            get
            {
                if (SpawnPointID == 0)
                {
                    Debug.LogWarning(" This Spawn Point does not have SpawnID Assigned", this);
                    return false;
                }
                else {
                    Spawner[] spawns = FindObjectsOfType<Spawner>();
                    foreach (Spawner spawn in spawns) {
                        if (SpawnPointID == spawn.SpawnPointID)
                            return false;
                    }

                }


                    return true;
            }
        }
        void AssignNumber() {
            if (CheckID)
                return;
            Spawner[] spawns = FindObjectsOfType<Spawner>();
            SpawnPointID = (uint)spawns.Length;
        }
    }
}
