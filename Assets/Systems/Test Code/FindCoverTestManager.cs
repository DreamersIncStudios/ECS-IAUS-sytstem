using System.Collections.Generic;
using DreamersInc.BestiarySystem;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Utilities;

public class FindCoverTestManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        for (int i = 0; i < 10; i++)
        {
            if (GlobalFunctions.RandomPoint(Vector3.zero, 25, out float3 pos))
                BestiaryDB.SpawnNPC(3, pos);
            else
            {
                i--;
            }

        }
        var entity = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity();
        World.DefaultGameObjectInjectionWorld.EntityManager.AddComponent<RunningTag>(entity);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
