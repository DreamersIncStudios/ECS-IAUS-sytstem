using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS.Consideration;
using IAUS;
namespace IAUS.ECS.ComponentStates
{
    public class MoveTo : MonoBehaviour, IConvertGameObjectToEntity
    {
        public List<Transform> Points;
        public float MaxRange = 75.0f;
        public float MinRange = 3.0f;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new MoveToComponent()
            {
                MaxRange=MaxRange,
                MinRange=MinRange,
                DistanceConsider = new ConsiderationBaseECS()
                {
                    responseType = ResponseTypeECS.Logistic,
                    M = 50,
                    K = .85f,
                    B = 0.15f,
                    C = .6f
                },
                WaitForConsider = new ConsiderationBaseECS
                {
                    responseType = ResponseTypeECS.Logistic,
                    M = 5,
                    K = -.99f,
                    B = 1.0f,
                    C = 0.6f
                },

                Target = Points[0].position
            };
            dstManager.AddComponentData(entity, data);
            if (Points.Count > 0)
            {
                dstManager.AddBuffer<Patrol>(entity);
                foreach (Transform Point in Points)
                {
                    dstManager.GetBuffer<Patrol>(entity).Add(Point.position);
                }
            }

        }

    }

    public struct MoveToComponent : IComponentData
    {
        public float Score;
        public ConsiderationBaseECS DistanceConsider;
        public ConsiderationBaseECS WaitForConsider;
        public Vector3 Target;
        public float Distance;
        public float MaxRange;
        public float MinRange;
        public float input (float dist){
           return Mathf.Clamp01((float)(dist - MinRange) / (float)(MaxRange - MinRange));
         }
    }
    // This is to be moved to the Guard/PatrolComponenet
    // This should be set by the Guard/Patrol COmponent
    [InternalBufferCapacity(12)]
    public struct Patrol : IBufferElementData
    {
        public static implicit operator Vector3(Patrol e) { return e.Value; }
        public static implicit operator Patrol(Vector3 e) { return new Patrol { Value = e }; }

        // Actual value each buffer element will store.
        public Vector3 Value;

    }

}