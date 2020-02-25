using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
namespace IAUS.ECS2
{
    public class DetectionSystem : JobComponentSystem
    {
        public NativeArray<Entity> AttackableEntityInScene;
        public EntityQueryDesc AttackableQuery = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Attackable), typeof(LocalToWorld) }
        };

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle handle = new GetListOfTarget()
            {
                attackableEntities = GetEntityQuery(AttackableQuery).ToEntityArray(Allocator.TempJob),
                Positions = GetComponentDataFromEntity<LocalToWorld>(true)

            }.Schedule(this, inputDeps);
          
            return handle;
        }
    }


    struct GetListOfTarget : IJobForEachWithEntity_EBC<TargetBuffer,Detection>
    {
        [DeallocateOnJobCompletion] [ReadOnly]public NativeArray<Entity> attackableEntities;
        [NativeDisableParallelForRestriction][ReadOnly] public ComponentDataFromEntity<LocalToWorld> Positions;

        public void Execute(Entity entity, int Tindex, DynamicBuffer<TargetBuffer> Target, ref Detection c1)
        {
            Target.Clear();

            for (int index = 0; index < Target.Length; index++) {
                float dist = Vector3.Distance(Positions[attackableEntities[index]].Position, Positions[entity].Position);
                if (dist <= c1.viewRadius)
                {
                    Vector3 dirToTarget = ((Vector3)Positions[attackableEntities[index]].Position - (Vector3)Positions[entity].Position).normalized;
                    if (Vector3.Angle(Positions[entity].Position, dirToTarget) < c1.viewAngleXZ / 2.0f) {
                        RaycastCommand tempRaycast = new RaycastCommand()
                        {
                            from = Positions[entity].Position,
                            direction = dirToTarget,
                            distance = dist,
                            layerMask = ~c1.ObstacleMask,
                            maxHits = 1
                        };
                        Target.Add(new TargetBuffer()
                        {
                            TargetToLookFor = new Target() {
                                target = attackableEntities[index],
                                RaycastCom = tempRaycast
                            }

                        } );

                    }
                }

            }
        }
    }
}