using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using TerrainCollider = UnityEngine.TerrainCollider;

namespace DreamersInc
{
    public class DOTSTerrainCollider : MonoBehaviour
    {
        public TerrainCollider terrainCollider;
        public PhysicsCategoryTags belongsTo;
        public PhysicsCategoryTags collideWith;

        class TerrainBaker : Baker<DOTSTerrainCollider> {
            public override void Bake(DOTSTerrainCollider authoring) {
                if (authoring.terrainCollider == null || authoring.terrainCollider.terrainData == null)
                { authoring.terrainCollider = GameObject.FindObjectOfType<TerrainCollider>(); }

                var data = authoring.terrainCollider.terrainData;
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
                        BelongsTo = authoring.belongsTo.Value,
                        CollidesWith = authoring.collideWith.Value,
                        GroupIndex = 0
                    });
                
                AddComponent(new PhysicsCollider()
                {
                    Value = colliders
                });
                AddSharedComponent( new PhysicsWorldIndex() { Value = 0 });


            }
        }

    }
}
