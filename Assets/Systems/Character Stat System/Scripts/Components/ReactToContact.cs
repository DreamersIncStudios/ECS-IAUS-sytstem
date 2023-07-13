using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace DreamersInc.CombatSystem.Animation
{
    public struct ReactToContact : IComponentData
    {
        public Direction HitDirection( out Vector3 dirToTarget) {
            dirToTarget = Vector3.Normalize(positionVector - HitContactPoint);

            float2 dot = new float2(Vector3.Dot(ForwardVector, dirToTarget),
                Vector3.Dot(RightVector, dirToTarget)
                );
            if (dot.x > dot.y) {
                if (dot.x >= .5)
                {
                    return Direction.Front;
                }
                if (dot.x <= -.5)
                {
                    return Direction.Back;
                }
            }
            else {
                if (dot.y >= .5)
                {
                    return Direction.Right;
                }
                if (dot.y <= -.5)
                {
                    return Direction.Left;
                }
            } 
            return Direction.none;
        }
        public float3 HitContactPoint;
        public float3 ForwardVector;
        public float3 RightVector;
        public float3 positionVector;
        public float HitIntensity; //Todo Figure out how to direction with stats

    }

    public enum Direction { none, Left, Right, Front, Back, Up, Down}
}
