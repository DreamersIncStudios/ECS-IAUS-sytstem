using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.SO;
public class TestSpawn : MonoBehaviour
{
    public NPCSO testing;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 500; i++)
        {
            testing.Spawn(this.transform.position);
        }
    }


}
