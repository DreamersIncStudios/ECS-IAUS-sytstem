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



    public struct Interactable : IComponentData, IEquatable<Interactable>
    {
        public InteractableType Type;
        public Weight Weight;

        public bool Equals(Interactable other)
        {
            return Type == other.Type && Weight == other.Weight;
        }

        public override bool Equals(object obj)
        {
            return obj is Interactable other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Type, (int)Weight);
        }
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