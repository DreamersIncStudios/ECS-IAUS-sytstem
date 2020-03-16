﻿
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;

namespace IAUS.ECS2
{

    public partial class DetectionSystem :ComponentSystem
    {
        float CheckHumanRays(NativeArray<RaycastHit> Results, Detection DetectSpecs)
        {
            float AlertLevel = new float();
            for (int index = 0; index < Results.Length; index++)
            {
                switch (index)
                {
                    case 0:
                        if (Hit(Results[index]))
                        {
                            AlertLevel +=.36f;
                        }
                        break;
                    case 1:
                        if (Hit(Results[index]))
                        {
                            AlertLevel += .09f;
                        }
                        break;
                    case 2:
                        if (Hit(Results[index]))
                        {
                            AlertLevel += .09f;
                        }
                        break;
                    case 3:
                        if (Hit(Results[index]))
                        {
                            AlertLevel += .09f;
                        }
                        break;
                    case 4:
                        if (Hit(Results[index]))
                        {
                            AlertLevel += .185f;
                        }
                        break;
                    case 5:
                        if (Hit(Results[index]))
                        {
                            AlertLevel += .185f;
                        }
                        break;
                }
            }
            return AlertLevel;
        }

        public bool Hit(RaycastHit Result)
        {
            Collider col = Result.collider;

            if (Result.collider != null)
            {
                return Result.collider.gameObject.layer == 11 ||
                    Result.collider.gameObject.layer == 12 ||
                    Result.collider.gameObject.layer == 10;

            }
            return false;
        }
    }
    
}
