//using System.Collections;
//using System.Collections.Generic;
//using System.Security.AccessControl;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Transforms;
//using UnityEngine;

//namespace DreamersInc.QuadrantSystems
//{
//    public  partial  struct QuadrantSystem : ISystem
//    {
//        private const int quadrantYMultiplier = 1000;
//        private const int quadrantCellSize = 50;
//        private static int GetPositionHashMapKey(float3 position) {
//            return (int)(math.floor(position.x / quadrantCellSize) + (quadrantYMultiplier * math.floor(position.y / quadrantCellSize)));
//        }
//        private int GetEntityCountInHashMap(NativeParallelMultiHashMap<int, Entity> quadrantMap, int hashMapKey) {
//            int count = 0;
//            if (quadrantMap.TryGetFirstValue(hashMapKey, out Entity entity, out NativeParallelMultiHashMapIterator<int> iterator))
//            {
//                do {
//                    count++;
//                }
//                while (quadrantMap.TryGetNextValue(out entity, ref iterator));
//            }
//            return count;
//        }
//        private static void DebugDrawQuadrant(float3 position) {
//            Vector3 lowerLeft = new Vector3(math.floor(position.x / quadrantCellSize) * quadrantCellSize, (quadrantYMultiplier * math.floor(position.y / quadrantCellSize) * quadrantCellSize));
//            Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+1, +0) * quadrantCellSize);
//            Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+0, +1) * quadrantCellSize);
//            Debug.DrawLine(lowerLeft + new Vector3(+1, +0) * quadrantCellSize, lowerLeft + new Vector3(+1, +1) * quadrantCellSize);
//            Debug.DrawLine(lowerLeft + new Vector3(+0, +1) * quadrantCellSize, lowerLeft + new Vector3(+0, +0) * quadrantCellSize);
//            Debug.Log(GetPositionHashMapKey(position) + "" + position);

//        }
//        public void OnCreate(ref SystemBase state) { }
//        public void OnUpdate(ref SystemBase state) {
//            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.TempJob).WithAll<LocalTransform>().Build(state);
//            NativeParallelMultiHashMap<int, Entity> quadrantMultiHashMap = new NativeParallelMultiHashMap<int, Entity>(entityQuery.CalculateEntityCount(), Allocator.TempJob);
//            new SetQuadrantDataHashMapJob() { quadrantMap = quadrantMultiHashMap.AsParallelWriter() }.ScheduleParallel();
//            quadrantMultiHashMap.Dispose();
//        }
//        public void OnDestroy(ref SystemBase state) { }


//        partial struct SetQuadrantDataHashMapJob : IJobEntity {
//            public NativeParallelMultiHashMap<int, Entity>.ParallelWriter quadrantMap;
//            public void Execute(Entity entity, ref LocalTransform transform) {
//                int hashMapKey = GetPositionHashMapKey(transform.Position);
//                quadrantMap.Add(hashMapKey, entity);
//            }
//        } 
//    }
    
//}