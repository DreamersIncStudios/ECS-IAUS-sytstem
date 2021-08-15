using System.Collections;
using System.Collections.Generic;
using Global.Component;
using IAUS.ECS2.Component;
using IAUS.ECS2;
using UnityEngine;
using Components.MovementSystem;
using AISenses;
using DreamersInc.InflunceMapSystem;
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
        TypeOfNPC GetTypeOfNPC { get; }
        Vision GetVision { get; }
        InfluenceComponent GetInfluence { get; }
    }

    public interface INPCEnemy {
        RetreatCitizen GetRetreat { get; } // change to RetreatEnemyNPC after testing

        bool IsPartOfTeam { get; }
        List<AttackTypeInfo> GetAttackType { get; }
        AttackTargetState GetAttackTargetState { get; }
    }

    public interface INPCCitizen {
        RetreatCitizen GetRetreat { get; }
}
    public interface INPCPlayable { 
        RetreatPlayerPartyNPC GetRetreat { get; }
    }

    public enum TypeOfNPC{  
        Neurtal, Friendly, Enemy

    }
}