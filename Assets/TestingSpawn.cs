using System.Collections;
using System.Collections.Generic;
using DreamersInc.DamageSystem.Interfaces;
using Unity.Entities;
using UnityEngine;
using NPC = DreamersInc.Bestiary.NPC;
public class TestingSpawn : MonoBehaviour
{
    [SerializeField] private NPC spawnData;

    [SerializeField] private Entity test;
    // Start is called before the first frame update
    void Start()
    {
        CharacterBuilder.CreateCharacter(spawnData.Name, out test).
            WithModel(spawnData.Prefab, new Vector3(0,1,0),"Enemy NPC").
            WithStats(spawnData.stats).
            WithAIControl(spawnData.AIStatesToAdd).
            WithAIMovement(spawnData.Move).
            WithAnimation().
            Build();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
