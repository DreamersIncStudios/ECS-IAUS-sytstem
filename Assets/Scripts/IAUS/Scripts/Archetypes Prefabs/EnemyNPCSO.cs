using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.AI;
using IAUS.NPCSO.Interfaces;
using Global.Component;
using IAUS.ECS;
using DreamersInc.InflunceMapSystem;
using IAUS.ECS.Component;
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
        public int GetFactionID { get { return getInfluence.factionID; } }
    

        public List<AttackTypeInfo> GetAttackType => getAttackTypes;
        [SerializeField] List<AttackTypeInfo> getAttackTypes;

        public AttackTargetState GetAttackTargetState => GetAttackTarget;
        [SerializeField] AttackTargetState GetAttackTarget;

        public void Setup(string Name, GameObject model, TypeOfNPC typeOf, AITarget self, Vision vision, List<AIStates> NpcStates, Movement movement, PatrolBuilderData patrol, WaitBuilderData wait,
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
            {
                EnemyCharacter enemyStats = SpawnedGO.AddComponent<EnemyCharacter>();
                enemyStats.SetAttributeBaseValue(10, 300, 100, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20);
                enemyStats.Name = GetName;
            }
            AIAuthoring.factionID = GetFactionID;

            if (isPartofTeam) {
                TeamAuthoring teamAuthoring = new TeamAuthoring() {
                    Info = getTeamInfo
              };

            }
            AIAuthoring.factionID = GetFactionID;
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

            if (GetAttackType.Count >0)
            {
                AIAuthoring.GetAttackType = GetAttackType;
                AIAuthoring.attackTargetState = GetAttackTargetState;
            }


        }
        public void Spawn(Vector3 pos, int SquadID) {
            Spawn(pos);
            SpawnedGO.AddComponent<SquadSetup>().SquadID = SquadID;
        }

    }
}