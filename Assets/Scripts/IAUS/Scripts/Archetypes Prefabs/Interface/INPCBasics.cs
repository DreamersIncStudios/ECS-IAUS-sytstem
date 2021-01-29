using System.Collections;
using System.Collections.Generic;
using Global.Component;
using IAUS.ECS2.Component;
using IAUS.ECS2;
using UnityEngine;
using Components.MovementSystem;
namespace IAUS.SO.Interfaces
{
    public interface INPCBasics
    {
        AITarget Self { get; }
        Movement AIMove { get; }
        List<AIStates> AIStatesAvailable { get; }
        GameObject Model { get; }
        Patrol GetPatrol { get; }
        Wait GetWait { get; }
        Retreat GetRetreat { get; }
        TypeOfNPC GetTypeOfNPC { get; }
        AISenses.Authoring.AISensesAuthoring GetAISenses { get; }
    }
    public enum TypeOfNPC{  
        Neurtal, Friendly, Enemy

    }
}