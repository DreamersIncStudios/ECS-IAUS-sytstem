using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamersInc.InflunceMapSystem;
using IAUS.NPCSO.Interfaces;
using Global.Component;
using AISenses;
using IAUS.ECS2;
using Components.MovementSystem;
using IAUS.ECS2.Component;

namespace IAUS.NPCSO
{
    public class FriendNPCSO : NPCSO, IInfluence
    {
        public InfluenceComponent GetInfluence { get { return getInfluence; } }

        [SerializeField] InfluenceComponent getInfluence;
        public Faction getFaction { get { return factionMember; } }
        [SerializeField] Faction factionMember;
        public void Setup(string Name, GameObject model, TypeOfNPC typeOf, InfluenceComponent GetInfluence, AITarget self, Vision vision, List<AIStates> NpcStates, Movement movement, Patrol patrol, Wait wait)
        {
            base.Setup(Name, model, typeOf, self, vision, NpcStates, movement, patrol, wait);
            this.getInfluence = GetInfluence;
        }
        public override void Spawn(Vector3 pos)
        {
            base.Spawn(pos);
            AIAuthoring.faction = getFaction;
        }

    }
}