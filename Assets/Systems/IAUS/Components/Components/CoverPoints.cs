using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public class CoverPoints : MonoBehaviour
    {
        [SerializeField] List<GameObject> coverPoints;

        private class CoverPointsBaker : Baker<CoverPoints>
        {
            public override void Bake(CoverPoints authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Cover(authoring.coverPoints));
            }
        }
    }

    public struct Cover : IComponentData, IEquatable<Cover>
    {
        public FixedList512Bytes<float3> CoverPoints;

        public Cover(List<GameObject> authoringCoverPoints)
        {
            CoverPoints = new FixedList512Bytes<float3>();
            foreach (var point in authoringCoverPoints)
                CoverPoints.Add(point.transform.position);
        }

        public bool Equals(Cover other)
        {
            return CoverPoints.Equals(other.CoverPoints);
        }

        public override bool Equals(object obj)
        {
            return obj is Cover other && Equals(other);
        }

        public override int GetHashCode()
        {
            return CoverPoints.GetHashCode();
        }
    }
}