using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

namespace DreamersInc
{
    public struct Player_Control : IComponentData
    {

        public bool InSafeZone;
    }
    public struct NPC_Control : IComponentData
    {

        public bool InSafeZone;
    }
}