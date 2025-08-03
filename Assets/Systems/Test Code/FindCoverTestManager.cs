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
        BestiaryDB.SpawnNPC(5, Vector3.zero);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
