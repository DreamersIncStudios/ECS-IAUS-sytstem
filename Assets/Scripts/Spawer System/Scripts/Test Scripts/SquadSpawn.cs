using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.SpawnerSystem;
using IAUS.ECS2;
using Utilities;
using Unity.Mathematics;

namespace SpawnerSystem.Test
{
    public class SquadSpawn : MonoBehaviour
    {
        public IAUSSquadSO Squad1;
        public List<PatrolBuffer> Points;
        // Start is called before the first frame update
        void Start()
        {
            UpdatePoint();
            Squad1.Spawn(this.transform.position, Points);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void UpdatePoint() {
            while (Points.Count < 5) {
                Vector3 position;
                if (GlobalFunctions.RandomPoint(transform.position, 35, out position))
                {
                    PatrolBuffer temp = new PatrolBuffer()
                    {
                        WayPoint = new PatrolPoint()
                        {
                            Point = (float3)position,
                            WaitTime =5
                        }
                    };
                    Points.Add(temp);

                }            
            
            }
        
        }
    }
}