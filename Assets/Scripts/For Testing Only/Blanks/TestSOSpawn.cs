using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectRebirth.Bestiary;
public class TestSOSpawn : MonoBehaviour
{

    public EnemyNPC spawntest;
    // Start is called before the first frame update
    void Start()
    {
        spawntest.Spawn(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
