using System;
using IAUS.Components.States;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IAUS.BlobAssets
{
    public struct WaypointData
    {
        public BlobArray<Waypoint> WaypointArray;
    }

    public partial class WaypointCreationSystem : SystemBase
    {
        private BlobAssetReference<WaypointData> waypointReference;

        protected override void OnCreate()
        {
            var waypoints = Object.FindObjectsByType<WaypointGO>((FindObjectsSortMode)FindObjectsInactive.Include);
            using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
            {
               ref var waypointBlobAsset = ref blobBuilder.ConstructRoot<WaypointData>();
               BlobBuilderArray<Waypoint> waypointArray =
                   blobBuilder.Allocate(ref waypointBlobAsset.WaypointArray, waypoints.Length);
               for (int i = 0; i < waypoints.Length; i++)
               {
                   waypointArray[i] = new Waypoint { Position = waypoints[i].transform.position };
               }

               waypointReference = blobBuilder.CreateBlobAssetReference<WaypointData>(Allocator.Persistent);
            }
       
        }

        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((ref PatrolArea patrolArea, in SetupIAUSBrainTag tag) =>
            {
                patrolArea.WaypointBlobRef = waypointReference;

            }).Run();
        }
    }
    
}