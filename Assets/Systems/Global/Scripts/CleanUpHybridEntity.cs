using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace Utilities.ECS
{

    public class CleanUpHybridEntity : ComponentSystem
    {
        protected override void OnUpdate()
        {

            Entities.ForEach((Entity entity, ref Destroytag tag) =>
            {
                PostUpdateCommands.DestroyEntity(entity);
            });
        }

    }

    public struct Destroytag : IComponentData {
        public float delay;
    }
}