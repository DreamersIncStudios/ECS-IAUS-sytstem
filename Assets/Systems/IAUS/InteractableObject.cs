using System;
using Unity.Entities;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public class InteractableObject : MonoBehaviour
    {
        [SerializeField] InteractableType Type;
        [SerializeField] Weight Weight;
         class Baker : Baker<InteractableObject>
        {
            public override void Bake(InteractableObject authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Interactable() { Type = authoring.Type, Weight = authoring.Weight});
            }
        }
    }

    public struct Interactable : IComponentData
    {
        public InteractableType Type;
        public Weight Weight;
    }

    public enum Weight
    {
        Light,
        Medium,
        Heavy,
    }

    [Flags]
    public enum InteractableType
    {
        None = 0,
        Damageable  = 1,
        Pickup = 2,
        Interactable= 4,
        Explosive = 8,
    }
}