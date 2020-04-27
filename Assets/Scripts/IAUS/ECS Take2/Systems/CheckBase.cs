using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using InfluenceMap.Factions;
using Unity.Collections;
using IAUS.Core;
namespace IAUS.ECS2 {
    [UpdateInGroup(typeof(IAUS_UpdateState))]

    public class CheckBase : JobComponentSystem
    {
        EntityQueryDesc BasesQuery = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Base), typeof(Attackable) }

        };

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ComponentDataFromEntity<LocalToWorld> transform = GetComponentDataFromEntity<LocalToWorld>(true);
            ComponentDataFromEntity<Attackable> FactionInfo = GetComponentDataFromEntity<Attackable>(true);
            NativeArray<Entity> BaseEntities = GetEntityQuery(BasesQuery).ToEntityArray(Allocator.TempJob);

            JobHandle test = Entities
                .WithNativeDisableParallelForRestriction(transform)
                .WithNativeDisableParallelForRestriction(FactionInfo)
                .WithDeallocateOnJobCompletion(BaseEntities)
                .WithReadOnly(transform)
                .WithReadOnly(BaseEntities)
                .WithReadOnly(FactionInfo)
                .ForEach((Entity entity, ref Patrol c1) =>
            {
                if (c1.HomeEntity != Entity.Null)
                { return; }
                Attackable Self = FactionInfo[entity];
                Entity ClosestBase = new Entity();
                float DistanceToClosest = new float();
                for (int i = 0; i < BaseEntities.Length; i++)
                {
                    if (Self.Faction == FactionInfo[BaseEntities[i]].Faction) {
                        float dist = Vector3.Distance(transform[entity].Position, transform[BaseEntities[i]].Position);
                        if (ClosestBase == Entity.Null)
                        {
                            ClosestBase = BaseEntities[i];
                            DistanceToClosest = dist;
                        }
                        else if (dist < DistanceToClosest) {
                            ClosestBase = BaseEntities[i];
                            DistanceToClosest = dist;
                        }
                    }
                }
                if (ClosestBase != Entity.Null)
                    c1.HomeEntity = ClosestBase;

            }).Schedule(inputDeps);

            return test;
        }
    }
}