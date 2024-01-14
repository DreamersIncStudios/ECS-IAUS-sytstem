using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace DreamersInc.Utils
{ 
    public class GridGeneric<TGridObject>
    {

        public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
        public class OnGridValueChangedEventArgs : EventArgs {
            public int x, y;
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
        public GridGeneric(int width, int height, float cellSize, Func<GridGeneric<TGridObject>, int, int, TGridObject> CreateGridObject)  {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            gridArray = new TGridObject[width, height];
            originPosition = Vector3.zero;
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    gridArray[x, y] = CreateGridObject(this, x,y);
                }
            }
            DrawDebugGrid();

        }

        public GridGeneric(int width, int height, float cellSize, Vector3 originPos, Func<GridGeneric<TGridObject>, int, int, TGridObject> CreateGridObject)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            gridArray = new TGridObject[width, height];
            this.originPosition = originPos;
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    gridArray[x, y] = CreateGridObject(this, x,y);
                }
            }
            DrawDebugGrid();
        }

        public void DrawDebugGrid()
        {

            debugTextArray = new TextMesh[width, height];
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    debugTextArray[x, y] = Utilities.CreateWorldText(gridArray[x, y].ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize / 2, cellSize / 2), 20, Color.white, TextAnchor.MiddleCenter);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
            OnGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) =>
            {
                debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
            };
        }

        private Vector3 GetWorldPosition(int x, int y) {
            return new Vector3(x, y) * cellSize+originPosition;
        }
       public void GetXY(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
            y = Mathf.FloorToInt((worldPosition -originPosition).y / cellSize);
        }

        public void SetGridObject(int x, int y, TGridObject value) {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                gridArray[x, y] = value;
                if (OnGridValueChanged != null) OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y }); 
            }
        }
        public void SetGridObject(Vector3 worldPosition, TGridObject Value) {
            
            GetXY(worldPosition, out int x, out int y);
            SetGridObject(x, y, Value);
        }
        public void TriggerGridObjectChanged(int x, int y) {
            if (OnGridValueChanged != null) OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y });

        }
        public TGridObject GetGridObject(int x, int y) {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                return gridArray[x, y];
            }
            else {
                return default(TGridObject);
            }
        }
        public TGridObject GetGridObject(Vector3 WorldPosition) {
            GetXY(WorldPosition, out int x, out int  y);
            return GetGridObject(x, y);
        }

    }

}