using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Stats
{
    public class DeathSystem : ComponentSystem
    {
        //TODO update 
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, Transform anim, ref EntityHasDiedTag tag) => {
                Debug.Log("Play Death Animation");
                Object.Destroy(anim.gameObject, .5f);
                EntityManager.DestroyEntity(entity);
            });
        }
    }

    public struct EntityHasDiedTag : IComponentData { 

    }
}