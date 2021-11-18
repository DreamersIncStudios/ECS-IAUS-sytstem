using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


public struct Presence : IComponentData
{
    public int Value;
    public int speedMod;
    public int CrouchMod;
    public int ConcealmentMod;
}
