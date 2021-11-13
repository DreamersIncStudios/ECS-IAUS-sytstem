using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
namespace DreamersInc.FactionSystem
{
    [GenerateAuthoringComponent]
    public struct FactionMember : IComponentData
    {
        public int id;
        [BurstDiscard]
       [SerializeField] public string FactionName => FactionManager.Database.GetFaction(id).name;
        
    }
}