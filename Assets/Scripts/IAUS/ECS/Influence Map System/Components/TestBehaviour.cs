using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace InfluenceMap {
    public class TestBehaviour : MonoBehaviour, IConvertGameObjectToEntity
    {
        GameObject Go;
        Collider collider;
        public float width, height;
        public int NumCells;
        public Vector2 CellSize;
        public List<Vector3> CellPositions;

        private void Awake()
        {

        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            createHeatMapTile();
            var gridSpecs = new GridComponent() { width = width, height = height, cellsize = CellSize };
            dstManager.AddComponentData(entity, gridSpecs);
            DynamicBuffer<Gridpoint> Buffer = dstManager.AddBuffer<Gridpoint>(entity);
            foreach (Vector3 Pos in CellPositions) {
                Buffer.Add(new Gridpoint()
                {
                    Position = Pos
                });
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }


        void createHeatMapTile() {
            Go = this.gameObject;
            collider = Go.GetComponent<Collider>();
            width = collider.bounds.size.x;
            height = collider.bounds.size.z;
            CellSize = new Vector2((float)width / (float)NumCells, (float)height / (float)NumCells);

            Vector3 StartPos = collider.bounds.center - new Vector3(collider.bounds.extents.x, Go.transform.position.y, collider.bounds.extents.z) + new Vector3(CellSize.x / 2.0f, 0, CellSize.y / 2.0f);
            for (int x = 0; x < NumCells; x++)
            {
                for (int y = 0; y < NumCells; y++)
                {
                    CellPositions.Add(StartPos + new Vector3(CellSize.x * x, Go.transform.position.y, CellSize.y * y)
                        );
                }

            }
        }

    }
}
