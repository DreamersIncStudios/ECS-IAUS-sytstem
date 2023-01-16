using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamersInc.Utils;

namespace DreamersInc.InflunceMapSystem
{

    public sealed class InfluenceGridMaster : MonoBehaviour
    {
        public static InfluenceGridMaster Instance;
        public GridGenericXZ<InfluenceGridObject> grid { get; private set; }
        private int width = 600;
        private int height = 600;
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

    }


}