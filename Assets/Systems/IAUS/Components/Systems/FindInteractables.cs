using System.Collections.Generic;
using IAUS.ECS.Component;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.Components.Systems
{
    [InternalBufferCapacity(6)]
    public struct InteractablesInRange : IBufferElementData
    {
            public InteractableType Type;
            public Weight Weight;
            public Entity Entity;
        
    }

    public partial struct FindInteractablesSystem : ISystem
    {

        EntityQuery query;
        private EntityQuery targets;

        void OnCreate(ref SystemState state)
        {
            query = SystemAPI.QueryBuilder().WithAll<LocalTransform,InteractablesInRange>().Build();
            targets = SystemAPI.QueryBuilder().WithAll<LocalTransform, Interactable>().Build();
        }

        void OnUpdate(ref SystemState state)
        {
            new FindInteractables()
            {
                Interactables = targets.ToComponentDataArray<Interactable>(Allocator.TempJob),
                InteractablesPosition = targets.ToComponentDataArray<LocalTransform>(Allocator.TempJob),
                InteractablesEntity = targets.ToEntityArray(Allocator.TempJob)
            }.Schedule(query);
        }
    }

    public partial struct FindInteractables : IJobEntity
    {
        [NativeDisableContainerSafetyRestriction]

    public NativeArray<LocalTransform> InteractablesPosition;
        public NativeArray<Interactable> Interactables;
        public NativeArray<Entity> InteractablesEntity;
        void Execute(DynamicBuffer< InteractablesInRange> data, in LocalTransform transform)
        {
            var index = IndexOfClosestInteractable(transform);
            data.Clear();
            foreach (var i in index)
            {
                
                data.Add(new InteractablesInRange()
                {
                    Type = Interactables[i].Type,
                    Weight = Interactables[i].Weight,
                    Entity = InteractablesEntity[i]
                });
            }

        }

        private const float Range = 50f;

        private List<int> IndexOfClosestInteractable(LocalTransform transform)
        {
            var index = new List<int>();
            for (var i = 0; i < InteractablesPosition.Length; i++)
            {
                var position = InteractablesPosition[i];
                var dist = Vector3.Distance(transform.Position, position.Position);
                if (dist > Range) continue;
                index.Add(i);
            }
            return index;
        }
    }
}