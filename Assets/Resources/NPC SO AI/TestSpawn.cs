using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.SO;
public class TestSpawn : MonoBehaviour
{
    public NPCSO testing;
    [Range(1,200)]
    public int spawnCNT;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < spawnCNT; i++)
        {
            testing.Spawn(this.transform.position);
        }
    }


}
