using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

//public class DetectionSystem : ComponentSystem
//{

//    public EntityQueryDesc Looker = new EntityQueryDesc()
//    {
//        All = new ComponentType[] { typeof(Detection), typeof(LocalToWorld) , typeof(RobberC)}
//    };

//    public EntityQueryDesc Targets = new EntityQueryDesc()
//    {
//        All = new ComponentType[] { typeof(LocalToWorld) },
//        Any= new ComponentType[] { typeof(CitizenC),typeof(Police)}, 

//    };

//    int interval = 2;
//    // Consider adding an if Exist loop to separate citizen from Cops 
//    protected override void OnUpdate()
//    {


//        Entities.With(GetEntityQuery(Looker)).ForEach((ref Detection DetectSpecs, ref LocalToWorld transform, ref RobberC robber) =>
//        {
//            //if (Time.frameCount % interval == 0)
//            //{
//            NativeList<Entity> TargetsInRange = new NativeList<Entity>(Allocator.Persistent);
//            NativeList<Entity> EnemiesInRange = new NativeList<Entity>(Allocator.Persistent);
//            ComponentDataFromEntity<CitizenC> Shoppers = GetComponentDataFromEntity<CitizenC>();
//            ComponentDataFromEntity<Police> Cops = GetComponentDataFromEntity<Police>();

//            LocalToWorld TempTransform = transform;
//            Detection Spec = DetectSpecs;


//            Entities.With(GetEntityQuery(Targets)).ForEach((Entity entity, ref LocalToWorld toWorld) =>
//        {
//            float dist = Vector3.Distance(TempTransform.Position, toWorld.Position);
//            if (Spec.viewRadius > dist)
//            {
//                Vector3 dirToTarget = ((Vector3)toWorld.Position - (Vector3)TempTransform.Position).normalized;
//                if (Vector3.Angle(TempTransform.Forward, dirToTarget) < Spec.viewAngleXZ && Vector3.Angle(TempTransform.Forward, dirToTarget) < Spec.viewAngleYZ)
//                {// angle in YZ  plane 
//                        if (!Physics.Raycast(TempTransform.Position, dirToTarget, dist, Spec.ObstacleMask))
//                    {
//                        TargetsInRange.Add(entity);
//                        if (Shoppers.Exists(entity))
//                        {
//                            if (Spec.distanceToClosetTarget > dist)
//                                Spec.distanceToClosetTarget = dist / (float)Spec.viewRadius;
//                        }
//                        if (Cops.Exists(entity))
//                        {
//                            if (Spec.distanceToClosetEnemy > dist)
//                                Spec.distanceToClosetEnemy = dist / (float)Spec.viewRadius;
//                        }
//                    }
//                    else
//                    {
//                        if (Shoppers.Exists(entity))
//                        {
//                            Spec.distanceToClosetTarget = 1000;
//                        }
//                        if (Cops.Exists(entity))
//                        {
//                            Spec.distanceToClosetEnemy = 1000;
//                        }

//                    }
//                }
//                else
//                {
//                    if (Shoppers.Exists(entity))
//                    {
//                        Spec.distanceToClosetTarget = 1000.0f;
//                    }
//                    if (Cops.Exists(entity))
//                    {
//                        Spec.distanceToClosetEnemy = 1000.0f;
//                    }
//                }
//            }
//            else
//            {
//                if (Shoppers.Exists(entity))
//                {
//                    Spec.distanceToClosetTarget = 1000.0f;
//                }
//                if (Cops.Exists(entity))
//                {
//                    Spec.distanceToClosetEnemy = 1000.0f;
//                }
//            }
//        });

//            DetectSpecs = Spec;
//            TargetsInRange.Dispose();
//            EnemiesInRange.Dispose();

//            robber.DistanceToCop = Mathf.Clamp01((float)Spec.distanceToClosetEnemy / Spec.viewRadius);
//            robber.DistnaceToTarget = Mathf.Clamp01((float)Spec.distanceToClosetTarget / Spec.viewRadius);

//        //}
//        });
//    }
//    }




namespace IAUS.ECS.Component
{
    public struct Detection : IComponentData
    {
        public float viewRadius;
        [Range(0, 360)]
        public float viewAngleXZ;
        [Range(0, 360)]
        public float viewAngleYZ;

        public float EngageRadius;
        [Range(0, 360)]
        public float EngageViewAngle; //TBA
        public LayerMask TargetMask;
        public LayerMask ObstacleMask;
        public float distanceToClosetEnemy;
        public float distanceToClosetTarget;

        public float AlertModifer;
    }
}