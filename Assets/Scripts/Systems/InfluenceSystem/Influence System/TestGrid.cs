using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamersInc.InflunceMapSystem;
using DreamersInc.Utils;

public class TestGrid : MonoBehaviour
{
    public static TestGrid testGrid; 
    public static GridGenericXZ<InfluenceGridObject> grid;
    private BoxCollider collider => this.GetComponent<BoxCollider>();
    private int width => Mathf.FloorToInt(collider.bounds.size.x);
    private int height => Mathf.FloorToInt(collider.bounds.size.z);
    private Vector3 center => collider.bounds.center - new Vector3((float)width / 2f, 0, (float)height / 2f);
    

    // Start is called before the first frame update
    void Awake()
    {
        if (!testGrid)
            testGrid = this;
        else
            DestroyImmediate(this.gameObject);

        grid = new GridGenericXZ<InfluenceGridObject>(width, height, 1f, center, (GridGenericXZ<InfluenceGridObject> g, int x, int z) => new InfluenceGridObject(g, x, z));
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
