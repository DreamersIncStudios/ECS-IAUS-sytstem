using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using IAUS.ECS.Component;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

struct TargetToRaycast : IBufferElementData
{
    public float3 location;
    public float distance;
    public Vector3 DirToTarget;
    public RaycastCommand CastRay;
    public static implicit operator float3(TargetToRaycast e) { return e.location; }
    public static implicit operator float(TargetToRaycast e) { return e.distance; }
    public static implicit operator Vector3(TargetToRaycast e) { return e.DirToTarget; }
    
    public static implicit operator TargetToRaycast(float3 e) { return new TargetToRaycast { location = e }; }
    public static implicit operator TargetToRaycast(float e) { return new TargetToRaycast { distance = e }; }
    public static implicit operator TargetToRaycast(Vector3 e) { return new TargetToRaycast { DirToTarget = e }; }

}

struct GetListOfTarget : IJobForEachWithEntity<Detection, LocalToWorld>
{
   [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<LocalToWorld> TargetsInScene;
   [NativeDisableParallelForRestriction] public BufferFromEntity<TargetToRaycast> lookup;
    //  [NativeDisableParallelForRestriction] public NativeList<RaycastCommand> Commands;
    //public Detection specs;
    //public LocalToWorld Pos;

    public void Execute(Entity entity, int index2, ref Detection specs, ref LocalToWorld Pos)
    {
 
        var buffer = lookup[entity];
        buffer.Clear();
        for (int index = 0; index < TargetsInScene.Length; index++)
        {
            float dist = Vector3.Distance(Pos.Position, TargetsInScene[index].Position);
            if (specs.viewRadius > dist)
            {
                Vector3 dirToTarget = ((Vector3)TargetsInScene[index].Position - (Vector3)Pos.Position).normalized;
                if (Vector3.Angle(Pos.Forward, dirToTarget) < specs.viewAngleXZ /2)
                {
                    RaycastCommand tempRaycast = new RaycastCommand()
                    {
                        from = Pos.Position,
                        direction = dirToTarget,
                        distance = dist,
                        layerMask = ~specs.ObstacleMask,
                        maxHits = 1
                    };
                    buffer.Add(new TargetToRaycast() {CastRay=tempRaycast,
                        DirToTarget = dirToTarget, distance = dist, location = Pos.Position });


                }
            }
        }
    }

}



public class DetectionTest : JobComponentSystem
{
    
    public EntityQueryDesc Looker = new EntityQueryDesc()
    {
        All = new ComponentType[] { typeof(Detection), typeof(LocalToWorld), typeof(RobberC) }
    };

    public EntityQueryDesc Targets = new EntityQueryDesc()
    {
        All = new ComponentType[] { typeof(LocalToWorld) },
        Any = new ComponentType[] { typeof(CitizenC), typeof(Police) },

    };

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        BufferFromEntity<TargetToRaycast> lookup = GetBufferFromEntity<TargetToRaycast>();
        NativeArray<LocalToWorld> TargetsInScene = GetEntityQuery(Targets).ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
        var Job1 = new GetListOfTarget()
        {
            lookup = GetBufferFromEntity<TargetToRaycast>(),
            TargetsInScene = TargetsInScene
        };

       return Job1.Schedule(this,inputDeps);

  

    }
}

[UpdateAfter(typeof(DetectionTest))]
public class DetectionTest2 : ComponentSystem
{
    public EntityQueryDesc Looker = new EntityQueryDesc()
    {
        All = new ComponentType[] { typeof(Detection), typeof(LocalToWorld) , typeof(RobberC)}
    };
    NativeArray<RaycastHit> results;
    protected override void OnUpdate()
    {
        Entities.With(GetEntityQuery(Looker)).ForEach((Entity entity, ref Detection DetectSpecs, ref LocalToWorld transform, ref RobberC robber) =>
        {
            DynamicBuffer<TargetToRaycast> buffer = EntityManager.GetBuffer<TargetToRaycast>(entity);
            NativeList<RaycastCommand> CastRay = new NativeList<RaycastCommand>(Allocator.Persistent);
            foreach (TargetToRaycast t in buffer) {
                CastRay.Add(t.CastRay);
            }

         results = new NativeArray<RaycastHit>(CastRay.Length,Allocator.Persistent);

            JobHandle handle = RaycastCommand.ScheduleBatch(CastRay, results, 1);
            handle.Complete();


            foreach (RaycastHit result in results) {
                if (result.collider != null)
                {
                 //   Debug.Log(result.collider.gameObject.name);
                   // Debug.Log(result.collider.gameObject.layer== DetectSpecs.ObstacleMask);
                }
    
            }
            buffer.Clear();
            CastRay.Dispose();
            results.Dispose();
        });

    }
}

