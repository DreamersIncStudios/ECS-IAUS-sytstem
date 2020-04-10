using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public class Base : IComponentData
{
    [Range(0, 350)]
    public float Health;
    [Range(0, 350)]
    public float Resources;
}
