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
            JobHandle tester = Entities.ForEach((ref Detection detect,ref DynamicBuffer<TargetBuffer> targets)=>{

    

            }).Schedule(inputDeps);

            return tester;

        }
    }


    struct GetListOfTarget : IJobForEachWithEntity<Detection, LocalToWorld>
    {
        public void Execute(Entity entity, int index, ref Detection c0, ref LocalToWorld c1)
        {
            throw new System.NotImplementedException();
        }
    }
}