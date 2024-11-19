using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;
namespace Utilities
{
    public static class GlobalFunctions
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
            for (var i = 0; i < 30; i++)
            {
                var randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
                if (!NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f,
                        1 << NavMesh.GetAreaFromName("Walkable"))) continue;
                result = hit.position;
                return true;
            }
            result = Vector3.zero;
            return false;
        }
        public static bool RandomPoint(float range, out Vector3 result)
        {
            for (var i = 0; i < 30; i++)
            {
                var randomPoint = Vector3.zero + UnityEngine.Random.insideUnitSphere * range;
                if (!NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, 
                        1<<NavMesh.GetAreaFromName("Walkable"))) continue;
                result = hit.position;
                return true;
            }
            result = Vector3.zero;
            return false;
        }

        public static bool RandomPoint(Vector3 center, float range, out float3 result)
        {
            for (var i = 0; i < 30; i++)
            {
                var randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
                if (!NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f,
                        NavMesh.AllAreas))
                    continue;
                result = (float3)hit.position;
                return true;
            }
            result = float3.zero;
            return false;
        }
    

        public static bool RandomPointAwayFromThreat(Vector3 PlayerLocation, Vector3 ThreatLocation, float range, out Vector3 result)        {
            for (var i = 0; i < 30; i++)
            {
                var direction = (ThreatLocation - PlayerLocation) / Vector3.Distance(ThreatLocation, PlayerLocation);
                var locAwayFromThreat = ThreatLocation + direction * (range + 10) + UnityEngine.Random.insideUnitSphere * 10;
                if (!NavMesh.SamplePosition(locAwayFromThreat, out var hit, 2.5f,
                        1 << NavMesh.GetAreaFromName("Walkable"))) continue;
                result = (float3)hit.position;
                return true;

            }
            result = float3.zero;
            return false;
        }


        public static bool RandomPointAwayFromThreat(float3 playerLocation, float3 threatLocation, float range, out float3 result)
        {

            for (var i = 0; i < 30; i++)
            {
                var direction = (threatLocation - playerLocation) / Vector3.Distance(threatLocation, playerLocation);
                Vector3 locAwayFromThreat = threatLocation + direction * (range + 10) + (float3)UnityEngine.Random.insideUnitSphere * 10;
                if (!NavMesh.SamplePosition(locAwayFromThreat, out var hit, 2.5f,
                        1<<NavMesh.GetAreaFromName("Walkable"))) continue;
                result = (float3)hit.position;
                return true;
            }
            result = float3.zero;
            return false;
        }


        public static bool CanAvoidThreat;

    }
}
