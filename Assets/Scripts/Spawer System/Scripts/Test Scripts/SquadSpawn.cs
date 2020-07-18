using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.SpawnerSystem;
using IAUS.ECS2;
using Utilities;
using Unity.Mathematics;
using System;

namespace SpawnerSystem.Test
{
    public class SquadSpawn : MonoBehaviour
    {
        public IAUSSquadSO Squad1;
        public List<PatrolBuffer> Points;
        public int NumOfSquads;
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(InvokeMethod(spawn, 2, NumOfSquads));


        }

        // Update is called once per frame
        void spawn()
        {
            UpdatePoint();
            Squad1.Spawn(this.transform.position, Points);
        }
        void UpdatePoint() {
            Points = new List<PatrolBuffer>();
            while (Points.Count < 5) {
                Vector3 position;
                if (GlobalFunctions.RandomPoint(transform.position, 60, out position))
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

        public IEnumerator InvokeMethod(Action method, float interval, int invokeCount)
        {
            for (int i = 0; i < invokeCount; i++)
            {
                method();

                yield return new WaitForSeconds(interval);
            }
        }
    }
}