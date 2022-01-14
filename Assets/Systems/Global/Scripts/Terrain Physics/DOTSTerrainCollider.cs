using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using TerrainCollider = UnityEngine.TerrainCollider;

namespace OrbitGames.DOTSController.TempSolution
{
    internal sealed class DOTSTerrainCollider : MonoBehaviour, IConvertGameObjectToEntity
    {
        public TerrainCollider terrainCollider;
        public PhysicsCategoryTags belongsTo;
        public PhysicsCategoryTags collideWith;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (terrainCollider == null || terrainCollider.terrainData == null)
                return;

            var data = terrainCollider.terrainData;
            var size = new int2(data.heightmapResolution, data.heightmapResolution);
            var delta = data.size.x / (size.x - 1);
            var scale = new float3(delta, 1f, delta);

            var heights = new NativeArray<float>(size.x * size.y * UnsafeUtility.SizeOf<float>(), Allocator.Temp);

            var index = 0;
            for (var i = 0; i < size.x; ++i)
            {
                for (var j = 0; j < size.y; ++j)
                {
                    heights[index] = data.GetHeight(j, i);
                    ++index;
                }
            }

            var colliders = Unity.Physics.TerrainCollider.Create(heights, size, scale,
                Unity.Physics.TerrainCollider.CollisionMethod.VertexSamples,
                new CollisionFilter()
                {
                    BelongsTo = belongsTo.Value,
                    CollidesWith = collideWith.Value,
                    GroupIndex = 0
                });

            dstManager.AddComponentData(entity, new PhysicsCollider()
            {
                Value = colliders
            });

        }
    }

}
