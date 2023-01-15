using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
namespace Dreamers.SquadSystem
{
   
    public struct Team : IBufferElementData
    {
        public Entity TeamMember;
        public bool IsLeader;
        public bool HasLeader;
    }
    [System.Serializable]
    public struct TeamInfo : IComponentData
    {
        public uint CurrentTeamSize;
        public uint MaxTeamSize; // make this var based on roles later
        public bool IsFilled=> CurrentTeamSize < MaxTeamSize;
    }
    public struct TeamUpActionTag : IComponentData { 
        readonly bool test;
    }

    public struct LeaderEntityTag : GroupData {
        public bool HasSquad { get; set; }
    }

    public struct GruntEntityTag : GroupData
    {
        public bool HasSquad { get; set; }
    }
    public interface GroupData : IComponentData { 
        public bool HasSquad { get; set; }
    }
    public struct updateDataTeam : IComponentData { };
}