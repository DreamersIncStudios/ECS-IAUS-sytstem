using Global.Component;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace DreamersInc.BestiarySystem
{
    public sealed partial class BestiaryDB : MonoBehaviour
    {
        private static Entity CreateEntity(EntityManager manager, Transform transform, string entityName = "")
        {

            EntityArchetype baseEntityArch = manager.CreateArchetype(
              typeof(LocalTransform),
              typeof(LocalToWorld)
              );
            Entity baseDataEntity = manager.CreateEntity(baseEntityArch);
            if (entityName != string.Empty)
                manager.SetName(baseDataEntity, entityName);
            else
                manager.SetName(baseDataEntity, "NPC Data");
            manager.SetComponentData(baseDataEntity, new LocalTransform()
            {
                Position = transform.position,
                Rotation = transform.rotation,
                Scale = 1
            });


            return baseDataEntity;
        }

        private static void AddPhysics(EntityManager manager, Entity entityLink, GameObject spawnedGO, PhysicsInfo physicsInfo)
        {
            PhysicsShape shape = new PhysicsShape();
            if (spawnedGO.GetComponent<UnityEngine.CapsuleCollider>())
            {
                shape = PhysicsShape.Capsule;
                goto create;
            }
            if (spawnedGO.GetComponent<UnityEngine.BoxCollider>())
            {
                shape = PhysicsShape.Box;
                goto create;
            }
            Debug.LogError("Physics Collider Type is missing");

            create:
            BlobAssetReference<Unity.Physics.Collider> spCollider = new BlobAssetReference<Unity.Physics.Collider>();
            switch (shape)
            {
                case PhysicsShape.Capsule:
                    UnityEngine.CapsuleCollider col = spawnedGO.GetComponent<UnityEngine.CapsuleCollider>();
                    spCollider = Unity.Physics.CapsuleCollider.Create(new CapsuleGeometry()
                    {
                        Radius = col.radius,
                        Vertex0 = col.center + new Vector3(0, col.height, 0),
                        Vertex1 = new float3(0, 0, 0)

                    }, new CollisionFilter()
                    {
                        BelongsTo = physicsInfo.BelongsTo.Value,
                        CollidesWith = physicsInfo.CollidesWith.Value,
                        GroupIndex = 0
                    });


                    break;
                case PhysicsShape.Box:
                    UnityEngine.BoxCollider box = spawnedGO.GetComponent<UnityEngine.BoxCollider>();
                    spCollider = Unity.Physics.BoxCollider.Create(new BoxGeometry()
                    {
                        Center = box.center,
                        Size = box.size,
                        Orientation = quaternion.identity,

                    }, new CollisionFilter()
                    {
                        BelongsTo = physicsInfo.BelongsTo.Value,
                        CollidesWith = physicsInfo.CollidesWith.Value,
                        GroupIndex = 0
                    });
                    manager.AddComponentData(entityLink, new PhysicsCollider()
                    { Value = spCollider });
                    break;
            }
            manager.AddSharedComponent(entityLink, new PhysicsWorldIndex());
            manager.AddComponentData(entityLink, new PhysicsCollider()
            { Value = spCollider });
            manager.AddComponentData(entityLink, new PhysicsInfo
            {
                BelongsTo = physicsInfo.BelongsTo,
                CollidesWith = physicsInfo.CollidesWith
            });
        }

    }
}
