using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace DreamersIncStudio.GAIACollective
{
    public partial struct GaiaLevelManagement : ISystem
    {
        EntityQuery managerQuery;

        public void OnCreate(ref SystemState state)
        {
            managerQuery = new EntityQueryBuilder(state.WorldUpdateAllocator).WithAll<GaiaLevelManager, LocalToWorld>()
                .Build(ref state);

        }

        public void OnUpdate(ref SystemState state)
        {
            new ManagerAssignment()
            {
                LevelManagers = managerQuery.ToComponentDataArray<GaiaLevelManager>(Allocator.TempJob),
                Managers = managerQuery.ToEntityArray(Allocator.TempJob),
                Transform = managerQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob)
            }.Schedule();
        }
        public partial struct ManagerAssignment : IJobEntity
        {
           [ReadOnly] public NativeArray<GaiaLevelManager> LevelManagers;
           [ReadOnly] public NativeArray<Entity> Managers;
           [ReadOnly]  public NativeArray<LocalToWorld> Transform;
            
            void Execute(ref GaiaSpawnBiome biome, in LocalToWorld localToWorld)
            {
                if(biome.Manager != Entity.Null) return;
                for (var i = 0; i < LevelManagers.Length; i++)
                {
                    var manager = LevelManagers[i];
                    var managerPos = Transform[i].Position;
                    var biomePos = localToWorld.Position;

                    // Square bounds check using half-extents from manager.Bounds (centered at managerPos)
                    var halfX = manager.Bounds.x * 0.5f;
                    var halfZ = manager.Bounds.y * 0.5f;
                    var dx = Mathf.Abs(biomePos.x - managerPos.x);
                    var dz = Mathf.Abs(biomePos.z - managerPos.z);
                    if (dx <= halfX && dz <= halfZ)
                    {
                        biome.Manager = Managers[i];
                        break;
                    }
                }
            }
        }
    }
}