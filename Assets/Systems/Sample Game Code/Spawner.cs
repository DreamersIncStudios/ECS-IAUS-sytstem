using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject Pawn;
    public List<Transform> SpawnPoints;
    int SpawnCount;
    public int MaxSpawnCount;
    public bool SpawnEnemies { get { return SpawnCount < MaxSpawnCount; } }

    public int EnemyStartLevel;
    int Level=1;

    public Transform Sphere;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
