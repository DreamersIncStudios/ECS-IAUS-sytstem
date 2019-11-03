using IAUS.ECS.Utilities;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using IAUS.ECS.Component;
using UnityEngine;
namespace IAUS.Sample.Archtypes
{
    [RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof(Rigidbody))]

    public class Robber : BaseAI
    {
        public int MaxCashOnHand;
        public int CarryLimit;
        public int TravelRange;
        [Header("Detection")]
        public float viewRadius = 200;
        [Range(0, 360)]
        public float viewAngleXZ = 120;
        [Range(0, 360)]
        public float viewAngleYZ = 60;

        public float EngageRadius = 50;
        [Range(0, 360)]
        public float EngageViewAngle; //TBA
        public LayerMask TargetMask;
        public LayerMask ObstacleMask;

        public List<Transform> WayPoints;

        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);
            //Add Additional Components Here;
            var Rob = new RobberC { HomePos = transform.position, TravelRange = TravelRange, MaxCashOnHand = MaxCashOnHand,
                CarryLimit = CarryLimit, Robbing = false, CashOnHand = 0 };
            var detect = new Detection() { viewRadius=viewRadius, viewAngleXZ=viewAngleXZ,viewAngleYZ=viewAngleYZ,
            EngageRadius= EngageRadius, EngageViewAngle= EngageViewAngle, TargetMask= TargetMask, ObstacleMask= ObstacleMask};
            dstManager.AddComponentData(entity, detect); 
            dstManager.AddComponentData(entity, Rob);
            dstManager.AddBuffer<Waypoint>(entity);
            dstManager.AddBuffer<TargetToRaycast>(entity);
            DynamicBuffer<Waypoint> buffer = dstManager.GetBuffer<Waypoint>(entity);
            
            foreach (Transform point in WayPoints) {
                buffer.Add(new Waypoint() { Point = point.position });
                    }

            var l = new Lurk() { };
            var E = new RunFromCops() { };
            var R = new ReturnHome() { };
            var theft = new Rob() { };

            dstManager.AddComponentData(entity, l);
            dstManager.AddComponentData(entity, E);
            dstManager.AddComponentData(entity, R);
            dstManager.AddComponentData(entity, theft);


        }
    }
}

namespace IAUS.ECS.Component
{
    public struct RobberC : IComponentData {
        public float3 HomePos;
        public bool Robbing;
        public bool Arrested;
        public int TravelRange;
        public int CashOnHand;
        public int MaxCashOnHand;
        public int boughtItem;
        public int CarryLimit;

        public float DistanceToCop;
        public float DistnaceToTarget;

    }

    public struct Waypoint : IBufferElementData {
        public float3 Point ;

        public static implicit operator float3(Waypoint e) { return e.Point; }
        public static implicit operator Waypoint(int e) { return new Waypoint {Point= e }; }

    }
}