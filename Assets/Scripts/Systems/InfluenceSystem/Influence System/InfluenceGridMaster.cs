using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamersInc.Utils;

namespace DreamersInc.InflunceMapSystem
{
    
    public class InfluenceGridMaster : MonoBehaviour
    {
        public static InfluenceGridMaster Instance;
        public static GridGenericXZ<InflunceGridObject> grid;
        private int width =200;
        private int height= 300;
        private Vector3 center => Vector3.zero - new Vector3(width/2, 0, height/2);

        public static FilterGroups Filters = new FilterGroups();
      



        [SerializeField]public List<GridGenericXZ<InflunceGridObject>> Grids;

        // Start is called before the first frame update
        void Awake()
        {
            if (!Instance)
                Instance = this;
            else
                DestroyImmediate(this.gameObject);
        }
        void Start() {
            grid = new GridGenericXZ<InflunceGridObject>(width, height, 1f, center, (GridGenericXZ<InflunceGridObject> g, int x, int z) => new InflunceGridObject(g, x, z));
        }
   
    }

  
}