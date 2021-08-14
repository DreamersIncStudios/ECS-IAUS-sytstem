using System;
using UnityEngine;

namespace DreamersInc.Utils
{

    public class GridGenericXZ<TGridObject>
    {

        public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
        public class OnGridValueChangedEventArgs : EventArgs
        {
            public int x, z;
        }

        private int width, height;
        private float cellSize;
        private TGridObject[,] gridArray;
        private Vector3 originPosition;
        private TextMesh[,] debugTextArray;

        /// <summary>
        /// Create Grid
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public GridGenericXZ(int width, int height, float cellSize, Func<GridGenericXZ<TGridObject>, int, int, TGridObject> CreateGridObject)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            gridArray = new TGridObject[width, height];
            originPosition = Vector3.zero;
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int z = 0; z < gridArray.GetLength(1); z++)
                {
                    gridArray[x, z] = CreateGridObject(this, x, z);
                }
            }
         //   DrawDebugGrid();

        }

        public GridGenericXZ(int width, int height, float cellSize, Vector3 originPos, Func<GridGenericXZ<TGridObject>, int, int, TGridObject> CreateGridObject)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            gridArray = new TGridObject[width, height];
            this.originPosition = originPos;
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int z = 0; z < gridArray.GetLength(1); z++)
                {
                    gridArray[x, z] = CreateGridObject(this, x, z);
                }
            }
           // DrawDebugGrid();
        }

        public void DrawDebugGrid()
        {

            debugTextArray = new TextMesh[width, height];
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int z = 0; z < gridArray.GetLength(1); z++)
                {
                    debugTextArray[x, z] = Utilities.CreateWorldText(gridArray[x, z].ToString(), null, GetWorldPosition(x, z) + new Vector3(cellSize / 2, cellSize / 2), 20, Color.white, TextAnchor.MiddleCenter);
                    Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z + 1), Color.white, 1000f);
                    Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x + 1, z), Color.white, 1000f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 1000f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 1000f);
            OnGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) =>
            {
                debugTextArray[eventArgs.x, eventArgs.z].text = gridArray[eventArgs.x, eventArgs.z]?.ToString();
            };
        }

        private Vector3 GetWorldPosition(int x, int z)
        {
            return new Vector3(x, 0,  z) * cellSize + originPosition;
        }
        public void GetXZ(Vector3 worldPosition, out int x, out int z)
        {
            x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
            z = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);
        }

        public void SetGridObject(int x, int z, TGridObject value)
        {
            if (x >= 0 && z >= 0 && x < width && z < height)
            {
                gridArray[x, z] = value;
                if (OnGridValueChanged != null) OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, z = z });
            }
        }
        public void SetGridObject(Vector3 worldPosition, TGridObject Value)
        {

            GetXZ(worldPosition, out int x, out int z);
            SetGridObject(x, z, Value);
        }
        public void TriggerGridObjectChanged(int x, int z)
        {
            if (OnGridValueChanged != null) OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, z = z });

        }
        public TGridObject GetGridObject(int x, int z)
        {
            if (x >= 0 && z >= 0 && x < width && z < height)
            {
                return gridArray[x, z];
            }
            else
            {
                return default(TGridObject);
            }
        }
        public TGridObject GetGridObject(Vector3 WorldPosition)
        {
            GetXZ(WorldPosition, out int x, out int z);
            return GetGridObject(x, z);
        }

    }
}