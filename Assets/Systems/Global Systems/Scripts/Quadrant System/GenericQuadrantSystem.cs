using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DreamersInc.QuadrantSystems
{
    [DisableAutoCreation]
    public partial class GenericQuadrantSystem : SystemBase
    {
        private NativeParallelMultiHashMap<int, QuadrantData> quadrantMultiHashMap;
        private const int quadrantYMultiplier = 1000;
        private const int quadrantCellSize = 100;
        public EntityQuery query;
        public static int GetPositionHashMapKey(float3 position)
        {
            return (int)(Mathf.Floor(position.x / quadrantCellSize) + (quadrantYMultiplier * Mathf.Floor(position.y / quadrantCellSize)));
        }
        public int GetEntityCountInHashMap(NativeParallelMultiHashMap<int, QuadrantData> quadrantMap, int hashMapKey)
        {
            int count = 0;
            if (quadrantMap.TryGetFirstValue(hashMapKey, out QuadrantData quadrantData, out NativeParallelMultiHashMapIterator<int> iterator))
            {
                do
                {
                    count++;
                }
                while (quadrantMap.TryGetNextValue(out quadrantData, ref iterator));
            }
            return count;
        }
        private static void DebugDrawQuadrant(float3 position)
        {
            Vector3 lowerLeft = new Vector3(Mathf.Floor(position.x / quadrantCellSize) * quadrantCellSize, (quadrantYMultiplier * Mathf.Floor(position.y / quadrantCellSize) * quadrantCellSize));
            Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+1, +0) * quadrantCellSize);
            Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+0, +1) * quadrantCellSize);
            Debug.DrawLine(lowerLeft + new Vector3(+1, +0) * quadrantCellSize, lowerLeft + new Vector3(+1, +1) * quadrantCellSize);
            Debug.DrawLine(lowerLeft + new Vector3(+0, +1) * quadrantCellSize, lowerLeft + new Vector3(+0, +0) * quadrantCellSize);
            Debug.Log(GetPositionHashMapKey(position) + "" + position);

        }
        protected override void OnCreate()
        {
            quadrantMultiHashMap = new NativeParallelMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);
            base.OnCreate();
        }
        protected override void OnDestroy()
        {
            quadrantMultiHashMap.Dispose();

            base.OnDestroy();
        }
        protected override void OnUpdate()
        {

            if (query.CalculateEntityCount() != quadrantMultiHashMap.Capacity)
            {
                quadrantMultiHashMap.Clear();
                quadrantMultiHashMap.Capacity = query.CalculateEntityCount();

                new SetQuadrantDataHashMapJob() { quadrantMap = quadrantMultiHashMap.AsParallelWriter() }.ScheduleParallel(query);
            }
        }


        [BurstCompile]
        partial struct SetQuadrantDataHashMapJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int, QuadrantData>.ParallelWriter quadrantMap;
            public void Execute(Entity entity, [ReadOnly]in LocalTransform transform)
            {
                int hashMapKey = GetPositionHashMapKey(transform.Position);
                quadrantMap.Add(hashMapKey, new QuadrantData { 
                    entity = entity,
                    position = transform.Position
                });
            }
        }
        public struct QuadrantData {
            public Entity entity;
            public float3 position;
        }
    }
}