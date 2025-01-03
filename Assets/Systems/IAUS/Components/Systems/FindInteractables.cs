using IAUS.ECS.Component;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.Components.Systems
{
    public partial struct FindInteractables : IJobEntity
    {
        public NativeArray<LocalTransform> InteractablesPosition;
        public NativeArray<Entity> Interactables;
        void Execute(ref TerrorizeAreaState data, in LocalTransform transform)
        {
            var index = IndexOfClosestInteractable(transform);
            data.InteractableEntity = Interactables[index];
            
        }

        internal int IndexOfClosestInteractable(LocalTransform transform)
        {
            float distance = 90000;
            var index = 0;
            for (var i = 0; i < InteractablesPosition.Length; i++)
            {
                var position = InteractablesPosition[i];
                var dist = Vector3.Distance(transform.Position, position.Position);
                if (!(dist < distance)) continue;
                index = i;
                distance = dist;
            }
            return index;
        }
    }
}