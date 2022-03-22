using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamersInc.InflunceMapSystem;
using IAUS.NPCScriptableObj.Interfaces;
using Global.Component;
using AISenses;
using IAUS.ECS;
using Components.MovementSystem;
using IAUS.ECS.Component;

namespace IAUS.NPCScriptableObj
{
    public class FriendNPCSO : NPCSO, IInfluence
    {
        public InfluenceComponent GetInfluence { get { return getInfluence; } }

        [SerializeField] InfluenceComponent getInfluence;
        public int GetFactionID { get { return factionMemberID; } }
        [SerializeField] int factionMemberID;
        public void Setup(string Name, GameObject model, TypeOfNPC typeOf, InfluenceComponent GetInfluence, AITarget self, Vision vision, List<AIStates> NpcStates, Movement movement, PMovementBuilderData patrol, WaitBuilderData wait)
        {
            base.Setup(Name, model, typeOf, self, vision, NpcStates, movement, patrol, wait);
            this.getInfluence = GetInfluence;
        }
        public override void Spawn(Vector3 pos)
        {
            base.Spawn(pos);
            AIAuthoring.factionID = GetFactionID;
        }

    }
}