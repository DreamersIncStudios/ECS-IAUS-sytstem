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
    public int TargetClass;
    public float3 location;
    public float distance;
    public Vector3 DirToTarget;
    public RaycastCommand CastRay;

    public static implicit operator int(TargetToRaycast e) { return e.TargetClass; }
    public static implicit operator float3(TargetToRaycast e) { return e.location; }
    public static implicit operator float(TargetToRaycast e) { return e.distance; }
    public static implicit operator Vector3(TargetToRaycast e) { return e.DirToTarget; }

    public static implicit operator TargetToRaycast(int e) { return new TargetToRaycast { TargetClass = e }; }
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
    public int EnemyTypeClassifier;

    public void Execute(Entity entity, int index2, ref Detection specs, ref LocalToWorld Pos)
    {
 
        var buffer = lookup[entity];
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
                    buffer.Add(new TargetToRaycast() {CastRay=tempRaycast, TargetClass = EnemyTypeClassifier,
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

    public EntityQueryDesc Targets1 = new EntityQueryDesc()
    {
        All = new ComponentType[] { typeof(LocalToWorld) },
        Any = new ComponentType[] { typeof(CitizenC) },

    };
    public EntityQueryDesc Targets2 = new EntityQueryDesc()
    {
        All = new ComponentType[] { typeof(LocalToWorld) },
        Any = new ComponentType[] { typeof(Police) },

    };
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        BufferFromEntity<TargetToRaycast> lookup = GetBufferFromEntity<TargetToRaycast>();
        NativeArray<LocalToWorld> TargetsInScene = GetEntityQuery(Targets1).ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
        NativeArray<LocalToWorld> TargetsInScene2 = GetEntityQuery(Targets2).ToComponentDataArray<LocalToWorld>(Allocator.TempJob);

        var Job1 = new GetListOfTarget()
        {
            lookup = GetBufferFromEntity<TargetToRaycast>(),
            TargetsInScene = TargetsInScene,
            EnemyTypeClassifier = 0
            
        };
        var Job2 = new GetListOfTarget()
        {
            lookup = GetBufferFromEntity<TargetToRaycast>(),
            TargetsInScene = TargetsInScene2,
            EnemyTypeClassifier = 1

};

        JobHandle Handle = Job1.Schedule(this,inputDeps);
        return Job2.Schedule(this, Handle);
  

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

    public bool Hit(RaycastHit Result, Detection DetectSpecs)
    {
        Collider col = Result.collider;

        if (Result.collider != null)
        {
            return col.gameObject.layer ==8;
        }
        return false;
    }

    protected override void OnUpdate()
    {
        Entities.With(GetEntityQuery(Looker)).ForEach((Entity entity, ref Detection DetectSpecs, ref LocalToWorld transform, ref RobberC robber) =>
        {
            DynamicBuffer<TargetToRaycast> buffer = EntityManager.GetBuffer<TargetToRaycast>(entity);
            NativeList<RaycastCommand> CastRayTarget = new NativeList<RaycastCommand>(Allocator.Persistent);
            NativeList<RaycastCommand> CastRayEnemy = new NativeList<RaycastCommand>(Allocator.Persistent);

            foreach (TargetToRaycast t in buffer) {
                switch (t.TargetClass) {
                    case 0:
                    CastRayTarget.Add(t.CastRay);
                        break;
                    case 1:
                        CastRayEnemy.Add(t.CastRay);
                        break;
            }

            }
            // Look Into Layer to get rid of separate raycast commands
            results = new NativeArray<RaycastHit>(CastRayTarget.Length,Allocator.Persistent);

            JobHandle handle = RaycastCommand.ScheduleBatch(CastRayTarget, results, 1);
            handle.Complete();

            float closestDistance = DetectSpecs.viewRadius;

            foreach (RaycastHit result in results) {
                if(Hit(result, DetectSpecs)){
                    if (closestDistance > result.distance)
                    {
                        closestDistance = result.distance;
                        DetectSpecs.distanceToClosetTarget = (float)result.distance / DetectSpecs.viewRadius;
                    }
                }
    
            }

            results = new NativeArray<RaycastHit>(CastRayEnemy.Length, Allocator.Persistent);

            JobHandle handle2 = RaycastCommand.ScheduleBatch(CastRayEnemy, results, 1);
            handle.Complete();

            closestDistance = DetectSpecs.viewRadius;

            foreach (RaycastHit result in results)
            {
                if (Hit(result, DetectSpecs))
                {
                    if (closestDistance > result.distance)
                    {
                        closestDistance = result.distance;
                        DetectSpecs.distanceToClosetEnemy = (float)result.distance / DetectSpecs.viewRadius;
                    }
                }

            }

            robber.DistnaceToTarget = DetectSpecs.distanceToClosetTarget;
            robber.DistanceToCop = DetectSpecs.distanceToClosetEnemy;
            buffer.Clear();
            CastRayTarget.Dispose();
            CastRayEnemy.Dispose();
            results.Dispose();
        });

    }
}

