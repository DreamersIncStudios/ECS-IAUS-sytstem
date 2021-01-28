using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace InfluenceSystem.Component {
    public class GridPointSpawner : MonoBehaviour,IConvertGameObjectToEntity
    {
        public static GridPointSpawner spawner;
        public DestoryGridSytstem destoryGridSytstem;
        private void Awake()
        {
            if (spawner == null)
                spawner = this;
            else
            {
                Debug.LogWarning("Two Gridpoint spawner systems in scene ");
                Destroy(this.gameObject);
            }
        }

        public EntityManager entityManager;
        void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            CreateGrid((float3)transform.position, 10,reference);

        }

        public void CreateGrid(float3 Centerpoint, int GridID, Entity center)
        {
            EntityArchetype prefab = entityManager.CreateArchetype(
             typeof(LocalToWorld),
             typeof(Translation),
             typeof(InfluenceGridPoint)

         );
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    var instance = entityManager.CreateEntity(prefab);
                    entityManager.SetComponentData(instance, new Translation
                    {
                        Value = Centerpoint + new float3(i - 50,  0, j - 50)
                    });
           
                    entityManager.SetComponentData(instance, new InfluenceGridPoint { GridmapID = GridID,
                        center = center,
                        GridPoint = new int2() { x=i, y= j}
                    });

                }
            }


        }


        public void Test() {

            DestoryGrid(10);
            Debug.Log("Destory launch");
        }
        public void DestoryGrid(int ID)
        {
            if (destoryGridSytstem == null)
            {
                destoryGridSytstem = World.DefaultGameObjectInjectionWorld.CreateSystem<DestoryGridSytstem>();
            }
            destoryGridSytstem.RunDestoryJobOnce(ID);
        }
        Entity reference;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            reference = entity;
        }
    }

   [DisableAutoCreation]
    public class DestoryGridSytstem : ComponentSystem
    {
        EntityQuery Grids;
        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            Grids = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(InfluenceGridPoint)), ComponentType.ReadOnly(typeof(Translation)) }
            });
        }

        protected override void OnUpdate()
        {
          JobHandle  inputDeps = new DestoryGridByID()
            {
                GridID = ID,
                EntityChunk = GetEntityTypeHandle(),
                CommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                GridpointChunk = GetComponentTypeHandle<InfluenceGridPoint>(true)

            }.ScheduleParallel(Grids);
            inputDeps.Complete();
        }

        int ID;

        
        public void RunDestoryJobOnce(int id ) {
            ID = id;
            OnUpdate();


        }


        public struct DestoryGridByID : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityChunk;
            [ReadOnly] public ComponentTypeHandle<InfluenceGridPoint> GridpointChunk;
            public int GridID;
            public EntityCommandBuffer.ParallelWriter CommandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                NativeArray<InfluenceGridPoint> Gridpoints = chunk.GetNativeArray(GridpointChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    if (Gridpoints[i].GridmapID == GridID)
                    {
                        CommandBuffer.DestroyEntity(chunkIndex, entities[i]);
                    }

                }
            }
        }
    }
}