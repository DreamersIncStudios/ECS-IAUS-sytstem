using DreamersInc.BestiarySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testing : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            BestiaryDB.SpawnNPC(1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
