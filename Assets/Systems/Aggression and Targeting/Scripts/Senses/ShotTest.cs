using AISenses.HearingSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ShotTest : MonoBehaviour,IConvertGameObjectToEntity
{
    Entity self;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        self = entity;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
