using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamersInc.InflunceMapSystem;

namespace DreamersInc.Utils.DebugTools
{
    public class RetreatTestingTool : MonoBehaviour
    {
        InfluenceGridObject currentInflunceGridObject;
        private Vector3 previousPos;
        bool NPCOffGrid => null == InfluenceGridMaster.Instance.grid.GetGridObject(transform.position) && currentInflunceGridObject != null;
        bool GridChanged(out InfluenceGridObject gridpoint)
        {
            gridpoint = InfluenceGridMaster.Instance.grid.GetGridObject(transform.position);
            if (gridpoint == null)
            {
                return false;
            }
            return currentInflunceGridObject != gridpoint;
        }

        // Start is called before the first frame update
        void Start()
        {
            previousPos = transform.position;
            currentInflunceGridObject = InfluenceGridMaster.Instance.grid.GetGridObject(transform.position);
            currentInflunceGridObject.AddValue(new Unity.Mathematics.int2(100, 100), 20, 8, Faction.Player);

        }

        // Update is called once per frame
        void Update()
        {

            if (transform.hasChanged && GridChanged(out InfluenceGridObject point))
            {
                currentInflunceGridObject.AddValue(new Unity.Mathematics.int2(-100, -100), 20, 8, Faction.Player);
                point.AddValue(new Unity.Mathematics.int2(100, 100), 20, 8, Faction.Player);

                previousPos = transform.position;
                currentInflunceGridObject = point;
            }
        }
    }
}