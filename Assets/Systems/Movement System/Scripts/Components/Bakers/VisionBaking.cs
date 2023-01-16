using AISenses;
using Global.Component;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;

namespace DreamersInc.MovementSys
{
    public class VisionBaking : MonoBehaviour
    {
        public float3 HeadPositionOffset;
        public float3 ThreatPosition;

        public float viewRadius;
        [Range(0, 360)]
        public int ViewAngle;
        public float EngageRadius;
        public float AlertModifer;
        [Header("Physics Info")]
        public PhysicsCategoryTags BelongsTo;
        public PhysicsCategoryTags CollidesWith;

        class baking : Baker<VisionBaking>
        {
            public override void Bake(VisionBaking authoring)
            {
                var data = new Vision() {
                    HeadPositionOffset = authoring.HeadPositionOffset,
                    ThreatPosition = authoring.ThreatPosition,
                    ViewAngle = authoring.ViewAngle,
                    EngageRadius = authoring.EngageRadius,
                    AlertModifer = authoring.AlertModifer,
                    viewRadius = authoring.viewRadius
                };
               AddComponent(data);
                AddComponent(new PhysicsInfo {
                    BelongsTo = authoring.BelongsTo,
                    CollidesWith= authoring.CollidesWith,
                });
            }
        }
        public Vector3 DirFromAngle(float angleInDegree, bool angleIsGlobal)
        {
            if (angleIsGlobal)
            { angleInDegree += transform.eulerAngles.y; }

            return new Vector3(Mathf.Sin(angleInDegree * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegree * Mathf.Deg2Rad));
        }
    }
}
