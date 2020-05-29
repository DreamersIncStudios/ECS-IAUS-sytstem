using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.SpawnerSystem;

namespace SpawnerSystem.Test
{
    public class SquadSpawn : MonoBehaviour
    {
        public IAUSSquadSO Squad1;
        // Start is called before the first frame update
        void Start()
        {
            Squad1.Spawn(this.transform.position);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}