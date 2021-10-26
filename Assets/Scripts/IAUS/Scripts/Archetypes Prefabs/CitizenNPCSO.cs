using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;
using IAUS.NPCSO.Interfaces;
using Global.Component;
using AISenses;
using IAUS.ECS;
using Components.MovementSystem;
using IAUS.ECS.Component;
using DreamersInc.InflunceMapSystem;

namespace IAUS.NPCSO
{
    public class CitizenNPCSO : NPCSO,IInfluence
    {
        //TODO Editor defaults Patrol and Wait states on 
        public InfluenceComponent GetInfluence { get { return getInfluence; } }
        [SerializeField] InfluenceComponent getInfluence;
        public Faction GetFaction { get { return Faction.NonCombative; } }
        public override void Spawn(Vector3 pos)
        {
            base.Spawn(pos);
            NPCChararacter npcStat = SpawnedGO.AddComponent<NPCChararacter>();
            npcStat.SetAttributeBaseValue(10, 300, 100, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20);
            npcStat.Name = GetName;

            AIAuthoring.faction = GetFaction;
            AIAuthoring.GetInfluence = GetInfluence;

        }
    }
}