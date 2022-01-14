using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace Dreamers.Global
{
    public class Spawnable : MonoBehaviour, IConvertGameObjectToEntity
    {
        [HideInInspector] public Entity reference;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            reference = entity;
            dstManager.AddComponentData(entity, new CopyTransformFromGameObject()); // Or CopyTransformToGameObject - Only if you need to sync transforms

        }
        EntityManager MGR;
        public void Start()
        {
            MGR = World.DefaultGameObjectInjectionWorld.EntityManager;

        }
        void Update()
        {
            if (!MGR.Exists(reference))
                Destroy(this.gameObject);


        }

        public void OnDestroy()
        {
            if (MGR.Exists(reference))
                MGR.AddComponent<DestroyTag>(reference);

        }
    }
    public class Destoy : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Spawnable spawn) =>
            {
                if (GetComponentDataFromEntity<DestroyTag>(true).HasComponent(spawn.reference) && spawn.gameObject)
                    Object.Destroy(spawn.gameObject);
            });
        }
    }


    public struct DestroyTag : IComponentData
    {

    }


}