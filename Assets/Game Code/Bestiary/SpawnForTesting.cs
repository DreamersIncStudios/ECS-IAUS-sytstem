using DreamersInc.BestiarySystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Utilities;

public class SpawnForTesting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        List<float3> positions = new List<float3>();
        while (positions.Count < 70)
        {
            if (GlobalFunctions.RandomPoint(Vector3.zero, 450, out float3 pos))
                positions.Add(pos);
        }
        for (int i = 0; i < 10; i++)
        {
        //    BestiaryDB.SpawnNPC(1);
            BestiaryDB.SpawnNPC(3);

        }
        for (int i = 0; i < 25; i++)
        {
            if (GlobalFunctions.RandomPoint(Vector3.zero, 150, out float3 pos))
                BestiaryDB.SpawnDummy(2, pos + (float3)Vector3.up * 2);
            else
                i--;
        }
        //for (int i = 0; i < 15; i++)
        //{
        //    if (GlobalFunctions.RandomPoint(Vector3.zero, 150, out float3 pos))
        //        BestiaryDB.SpawnDummy(3, pos + (float3)Vector3.up * 2);
        //    else
        //        i--;
        //}
        //for (int i = 0; i < 6; i++)
        //{
        //    if (GlobalFunctions.RandomPoint(Vector3.zero, 150, out float3 pos))
        //        BestiaryDB.SpawnDummy(1, pos + (float3)Vector3.up * 2);
        //    else
        //        i--;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
