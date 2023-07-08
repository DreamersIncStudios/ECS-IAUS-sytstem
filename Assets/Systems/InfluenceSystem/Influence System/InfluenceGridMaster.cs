using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamersInc.Utils;
using PixelCrushers.LoveHate;
using Unity.Mathematics;

namespace DreamersInc.InflunceMapSystem
{

    public sealed class InfluenceGridMaster : MonoBehaviour
    {
        public static InfluenceGridMaster Instance;
        public GridGenericXZ<InfluenceGridObject> grid { get; private set; }
        private int width = 1100;
        private int height = 1100;
        private Vector3 center => Vector3.zero - new Vector3(width / 2, 0, height / 2);



        // Start is called before the first frame update
        void Awake()
        {
            if (!Instance)
                Instance = this;
            else
                DestroyImmediate(this.gameObject);
        }

        void Start()
        {
            grid = new GridGenericXZ<InfluenceGridObject>(width, height, 1f, center, (GridGenericXZ<InfluenceGridObject> g, int x, int z) => new InfluenceGridObject(g, x, z));

        }


        public int GetThreatLevel(Faction faction, Vector3 position, int areaSize = 10)
        {
            int2 value = grid.GetGridObject(position).GetAverageValue(faction, areaSize);
            float ratio = value.x / value.y;
            return Mathf.FloorToInt(ratio);
        }
        public int GetThreatLevel(int factionID, Vector3 position, int areaSize = 10)
        {

            return GetThreatLevel(LoveHate.GetFaction(factionID), position, areaSize);
        }
    }


}