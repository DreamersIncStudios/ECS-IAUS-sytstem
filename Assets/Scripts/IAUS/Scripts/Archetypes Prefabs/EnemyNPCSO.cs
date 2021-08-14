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
namespace IAUS.NPCSO {
    public class EnemyNPCSO : NPCSO, INPCEnemy
    {
        public bool IsPartOfTeam => isPartofTeam;
        [SerializeField] bool isPartofTeam = false;
        public InfluenceComponent GetInfluence => _getInflunce;
        [SerializeField] InfluenceComponent _getInflunce;
        public TeamInfo GetTeamInfo => getTeamInfo;
        [SerializeField] TeamInfo getTeamInfo;
        public RetreatCitizen GetRetreat => getRetreat;
        [SerializeField] RetreatCitizen getRetreat;
        public List<AttackTypeInfo> GetAttackType => getAttackTypes;
        [SerializeField] List<AttackTypeInfo> getAttackTypes;

        public AttackTargetState GetAttackTargetState => GetAttackTarget;
        [SerializeField] AttackTargetState GetAttackTarget;

        public  void Setup(bool team, InfluenceComponent influence, TeamInfo teamInfo, List<AttackTypeInfo> attackTypeInfos, RetreatCitizen flee)
        {
            isPartofTeam = team;
            _getInflunce = influence;
            getTeamInfo = teamInfo;
            getAttackTypes = attackTypeInfos;
            getRetreat = flee;
        }

        public override void Spawn(Vector3 pos)
        {
            base.Spawn(pos);
            if (Self.Type == TargetType.Character)
                SpawnedGO.AddComponent<EnemyCharacter>();
            if (isPartofTeam) {
                TeamAuthoring teamAuthoring = new TeamAuthoring() {
                    Info = getTeamInfo,

                    
              };

            }
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