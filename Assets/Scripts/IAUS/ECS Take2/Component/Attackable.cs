using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
namespace IAUS.ECS2
{
    [GenerateAuthoringComponent]
    public struct Attackable : IComponentData
    {
        public Factions Faction;
        public ObjectType Type;

        // system to get this points Break out to subclass


    }
    public enum ObjectType {
        Structure,
        Creature,
        Humaniod,
    }
    public enum Factions {
        Human,
        Angel,
        Daemon
    }
}
