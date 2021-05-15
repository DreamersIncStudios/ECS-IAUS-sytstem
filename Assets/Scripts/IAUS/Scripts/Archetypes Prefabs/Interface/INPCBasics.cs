using System.Collections;
using System.Collections.Generic;
using Global.Component;
using IAUS.ECS2.Component;
using IAUS.ECS2;
using UnityEngine;
using Components.MovementSystem;
using AISenses;
using InfluenceSystem.Component;
namespace IAUS.NPCSO.Interfaces
{
    public interface INPCBasics
    {
        string GetName { get; }
        AITarget Self { get; }
        Movement AIMove { get; }
        List<AIStates> AIStatesAvailable { get; }
        GameObject Model { get; }
        Patrol GetPatrol { get; }
        Wait GetWait { get; }
        Retreat GetRetreat { get; }
        TypeOfNPC GetTypeOfNPC { get; }
        Vision GetVision { get; }
        Hearing GetHearing { get; }
        Influence GetInfluence { get; }
    }
    public enum TypeOfNPC{  
        Neurtal, Friendly, Enemy

    }
}