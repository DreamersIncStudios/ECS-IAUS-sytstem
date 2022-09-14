using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Global.Component;
using Unity.Collections;

public partial class PhysicSystemLink : SystemBase
{
    EntityQuery targets;
    protected override void OnCreate()
    {
        base.OnCreate();
        targets = GetEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] { ComponentType.ReadOnly(typeof(LocalToParent)),
            ComponentType.ReadWrite(typeof(Rotation)),
            ComponentType.ReadWrite(typeof(Translation)),
            ComponentType.ReadOnly(typeof(AITarget))
            }

        });

    }

    protected override void OnUpdate()
    {
        Entities.WithoutBurst().WithAll<AITarget>().WithChangeFilter<LocalToWorld>().ForEach(( Transform transform, ref Translation tran, ref Rotation rot) => {
            tran.Value = transform.position;
            rot.Value = transform.rotation;
        
        }).Run();
    }


 
}

