using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;
namespace Utilities
{
    public class GlobalFunctions
    {
        /// <summary>
        /// Finds random location on NavMesh with a given range. 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="range"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool RandomPoint(Vector3 center, float range, out Vector3 result)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }
            result = Vector3.zero;
            return false;
        }


        public static bool RandomPoint(Vector3 center, float range, out float3 result)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                {
                    result = (float3)hit.position;
                    return true;
                }
            }
            result = float3.zero;
            return false;
        }


        public static bool RandomPointAwayFromThreat(Vector3 PlayerLocation, Vector3 ThreatLocation,  float range, out Vector3 result) {
            for (int i = 0; i < 30; i++)
            {
                Vector3 direction = (ThreatLocation - PlayerLocation) / Vector3.Distance(ThreatLocation, PlayerLocation);
                Vector3 LocAwayFromThreat = ThreatLocation + direction * (range + 10) + UnityEngine.Random.insideUnitSphere * 10;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(LocAwayFromThreat, out hit, 2.5f, NavMesh.AllAreas))
                {
                    result = (float3)hit.position;
                    return true;
                }

            }
            result = float3.zero;
            return false;
        }


        public static bool RandomPointAwayFromThreat(float3 PlayerLocation, float3 ThreatLocation , float range, out float3 result) {

            for (int i = 0; i < 30; i++)
            {
                float3 direction = (ThreatLocation - PlayerLocation) / Vector3.Distance(ThreatLocation, PlayerLocation);
                Vector3 LocAwayFromThreat = ThreatLocation + direction * (range + 10) + (float3)UnityEngine.Random.insideUnitSphere * 10;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(LocAwayFromThreat, out hit, 2.5f, NavMesh.AllAreas))
                {
                    result = (float3)hit.position;
                    return true;
                }
            }
            result = float3.zero;
            return false;
        }
      
        
        public static bool CanAvoidThreat;

    }
}
