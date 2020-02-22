using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.AI;
using Unity.Collections;
using Unity.Jobs;

namespace InfluenceMap
{
    //    [UpdateAfter(typeof(SetInfluences))]
    //    public class GruntTestBehaviour : JobComponentSystem
    //    {

    //        EntityQueryDesc Player = new EntityQueryDesc()
    //        {
    //            All = new ComponentType[] { typeof(PlayerCharacter), typeof(LocalToWorld) }
    //        };

    //        EntityQueryDesc Grid = new EntityQueryDesc()
    //        {
    //            All = new ComponentType[] { typeof(GridComponent) }
    //        };
    //        NativeList<Gridpoint> gridpoints = new NativeList<Gridpoint>(Allocator.TempJob);

    //        EntityCommandBuffer entityCommandBuffer;
    //        protected override void OnCreate()
    //        {
    //            base.OnCreate();
    //            entityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer();
    //        }

    //        protected override JobHandle OnUpdate(JobHandle inputDeps)
    //        {
    //            if (gridpoints.IsCreated)
    //                gridpoints.Dispose();
    //            gridpoints = new NativeList<Gridpoint>(Allocator.TempJob);
    //            // in actaully system add disposal system;
    //            var get = new GetGridPointWithValueOnly()
    //            {
    //                GridpointWithValue = gridpoints
    //            };


    //            JobHandle job1 = get.Schedule(this, inputDeps);
    //            job1.Complete();

    //            var mover = new MoveToGridPoint()
    //            {
    //                GridPointsInRange = get.GridpointWithValue,
    //                PlayerCharPositions = GetEntityQuery(Player).ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
    //                entityCommandBuffer = entityCommandBuffer
    //        };
    //            JobHandle job2 = mover.Schedule(this,job1);
    //            job2.Complete();


    //            return job2;
    //        }
    //        protected override void OnDestroy()
    //        {
    //            base.OnDestroy();
    //            if (gridpoints.IsCreated)
    //                gridpoints.Dispose();
    //        }
    //    }

    //Add a tag for this to run when needed
    [Unity.Burst.BurstCompile]
    public struct GetGridPointWithValueOnly : IJobForEach_B<Gridpoint>
    {
        [NativeDisableParallelForRestriction] public NativeList<Gridpoint> GridpointWithValue;
        public void Execute(DynamicBuffer<Gridpoint> buffer)
        {
                for (int index = 0; index < buffer.Length; index++)
                {
                    if (buffer[index].Player.Proximity.x > 0.0f)
                        GridpointWithValue.Add(buffer[index]);
                }
        }
    }
    [Unity.Burst.BurstCompile]
    public struct MoveToGridPoint : IJobForEachWithEntity<EnemyCharacter, Influencer, LookForPlayer>
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<Gridpoint> GridPointsInRange;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<LocalToWorld> PlayerCharPositions;
        [NativeDisableParallelForRestriction] public EntityCommandBuffer entityCommandBuffer;

        public void Execute(Entity entity, int JobIndex, ref EnemyCharacter c0, ref Influencer influencer, ref LookForPlayer Look)
        {
            for (int index = 0; index < GridPointsInRange.Length; index++)
            {
                for (int index2 = 0; index2 < PlayerCharPositions.Length; index2++)
                {
                    float dist = Vector3.Distance(PlayerCharPositions[index2].Position, GridPointsInRange[index].Position);

                    if (dist < 2.0f)
                    {
                        if (GridPointsInRange[index].Player.Proximity.x > influencer.influence.Proximity.x)
                        {
                           // c0.Target = PlayerCharPositions[index2];
                            entityCommandBuffer.RemoveComponent<LookForPlayer>(entity);
                            entityCommandBuffer.AddComponent<MoveToPlayer>(entity);

                            return;
                        }
                    }
                }
            }
        }
    }
}