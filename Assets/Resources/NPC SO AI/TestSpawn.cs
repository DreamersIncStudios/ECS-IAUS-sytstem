using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.SO;
public class TestSpawn : MonoBehaviour
{
    public NPCSO testing;
    [Range(1,2000)]
    public int spawnCNT;
    public int spawned;

    // Start is called before the first frame update
    void Start()
    {
        spawned = new int();
        InvokeRepeating("Spawn", 0, 2);
    }

    private void Update()
    {
        if (spawned > spawnCNT)
            CancelInvoke();
    }

    void Spawn()
    {
        testing.Spawn(this.transform.position);
        testing.Spawn(this.transform.position);
        testing.Spawn(this.transform.position);

        spawned+=3;
    }

}
