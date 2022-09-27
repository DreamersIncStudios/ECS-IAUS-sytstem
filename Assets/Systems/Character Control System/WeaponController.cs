using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Experimental.AI;
using DreamersInc.ComboSystem.NPC;
using Unity.Transforms;
using Unity.Physics;

public class WeaponController : MonoBehaviour
{
    [SerializeField]WeaponType weaponType;
    public void SpawnEntityData(Entity parent, int cnt) {

        EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityArchetype weaponArch = em.CreateArchetype(
              typeof(Translation),
              typeof(Rotation),
              typeof(LocalToWorld),
              typeof(CopyTransformFromGameObject),
              typeof(PhysicsCollider),
              typeof(PhysicsWorldIndex),
              typeof(Parent),
              typeof(WeaponData)
              );

        Entity weaponData = em.CreateEntity(weaponArch);
        em.SetName(weaponData, em.GetName(parent) + $" weapon {cnt}");
        em.SetComponentData(weaponData, new Parent { Value = parent });
        em.SetComponentData(weaponData, new WeaponData
        {
            weaponType = this.weaponType
        });

        em.AddComponentObject(weaponData, this.transform);

        em.AddComponentObject(weaponData, this.gameObject);
        DynamicBuffer<Child> children = em.GetBuffer<Child>(parent);
        children.Add(new Child { Value = weaponData });
    }
}

public struct WeaponData : IComponentData {
    public WeaponType weaponType;
}
public enum WeaponType
{ 
    Laser, Sniper, Assualt, Cannon, 
}
