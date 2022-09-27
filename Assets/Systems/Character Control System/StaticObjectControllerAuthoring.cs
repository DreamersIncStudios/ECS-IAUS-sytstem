using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;

public class StaticObjectControllerAuthoring : MonoBehaviour
{
    List<WeaponController> attachedWeapons;
    public void SetupControllerEntityData(Entity Data){
        EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
        em.AddComponentData(Data, new TowerController { 
           Position = this.transform.position,
           Attack=false,
           AimingAtTarget=false,
        });
        if (GetAttachedWeapons()) {
            DynamicBuffer<Child> children = em.AddBuffer<Child>(Data);
                foreach (var weapon in attachedWeapons) {
                 weapon.SpawnEntityData(Data, attachedWeapons.IndexOf(weapon));
                } 
        }

 
    }
    public bool GetAttachedWeapons() { 
    attachedWeapons = new List<WeaponController>();
        WeaponController[] weaponControllers = GetComponentsInChildren<WeaponController>();
        foreach (var item in weaponControllers)
        {
            attachedWeapons.Add(item);
        }
        return attachedWeapons.Count > 0;
    }
}
public struct TowerController : IComponentData
{
    public float3 TargetLocation;
    public float3 Position;
    public float3 dirToTarget;
    public float3 forward;
    public float RotateSpeed;
    public bool Attack;
    public bool AimingAtTarget;
}