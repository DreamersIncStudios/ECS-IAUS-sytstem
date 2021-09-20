using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.AI;
using IAUS.NPCSO.Interfaces;
using Global.Component;
using IAUS.ECS2;
using DreamersInc.InflunceMapSystem;
using IAUS.ECS2.Component;
using Stats;
using AISenses.Authoring;
using AISenses;
using Dreamers.SquadSystem;
using Components.MovementSystem;

namespace IAUS.NPCSO {
    public sealed class EnemyNPCSO : NPCSO, INPCEnemy,IInfluence
    {
        public bool IsPartOfTeam => isPartofTeam;
        [SerializeField] bool isPartofTeam = false;
        public TeamInfo GetTeamInfo => getTeamInfo;
        [SerializeField] TeamInfo getTeamInfo;
        public RetreatCitizen GetRetreat => getRetreat;
        [SerializeField] RetreatCitizen getRetreat;
        public InfluenceComponent GetInfluence { get { return getInfluence; } }

        [SerializeField] InfluenceComponent getInfluence;
        public Faction getFaction { get { return factionMember; } }
        [SerializeField] Faction factionMember;

        public List<AttackTypeInfo> GetAttackType => getAttackTypes;
        [SerializeField] List<AttackTypeInfo> getAttackTypes;

        public AttackTargetState GetAttackTargetState => GetAttackTarget;
        [SerializeField] AttackTargetState GetAttackTarget;

        public void Setup(string Name, GameObject model, TypeOfNPC typeOf, AITarget self, Vision vision, List<AIStates> NpcStates, Movement movement, Patrol patrol, Wait wait,
            bool team, TeamInfo teamInfo, List<AttackTypeInfo> attackTypeInfos, RetreatCitizen flee, InfluenceComponent influence)
        {
            base.Setup(Name, model, typeOf, self, vision, NpcStates, movement, patrol, wait);
            isPartofTeam = team;
            getTeamInfo = teamInfo;
            getAttackTypes = attackTypeInfos;
            getRetreat = flee;
            this.getInfluence = influence;
        }

        public override void Spawn(Vector3 pos)
        {
            base.Spawn(pos);
            if (Self.Type == TargetType.Character)
                SpawnedGO.AddComponent<EnemyCharacter>();
            AIAuthoring.faction = getFaction;

            if (isPartofTeam) {
                TeamAuthoring teamAuthoring = new TeamAuthoring() {
                    Info = getTeamInfo
              };

            }
            AIAuthoring.GetInfluence = getInfluence;
            foreach (AIStates state in AIStatesAvailable)
            {
                switch (state)
                {
                    case AIStates.Retreat:
                        AIAuthoring.AddRetreat = true;
                        AIAuthoring.retreatState = GetRetreat;
                    break;
                }
            }

            if (GetAttackType.Count >= 1)
            {
                AIAuthoring.GetAttackType = GetAttackType;
                AIAuthoring.attackTargetState = GetAttackTargetState;
            }
            else
                AIAuthoring.GetAttackType = new List<AttackTypeInfo>();

        }

    }
}