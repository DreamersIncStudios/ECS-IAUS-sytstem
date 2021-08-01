using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.AI;
using IAUS.NPCSO.Interfaces;
using Global.Component;
using IAUS.ECS2;
using Components.MovementSystem;
using IAUS.ECS2.Component;
using Stats;
using AISenses.Authoring;
using InfluenceSystem.Component;
using AISenses;
using Dreamers.SquadSystem;
namespace IAUS.NPCSO {
    public class EnemyNPCSO : NPCSO, INPCEnemy
    {
        public bool IsPartOfTeam => isPartofTeam;
        [SerializeField] bool isPartofTeam = false;
        public NPCLevel GetNPCLevel => _getNPCLevel;
        [SerializeField] NPCLevel _getNPCLevel;
        public bool IsLeader => (int)_getNPCLevel > 2;
        public TeamInfo GetTeamInfo => getTeamInfo;
        [SerializeField] TeamInfo getTeamInfo;
        public RetreatCitizen GetRetreat => getRetreat;
        [SerializeField] RetreatCitizen getRetreat;
        public List<AttackTypeInfo> GetAttackType => getAttackTypes;
        [SerializeField] List<AttackTypeInfo> getAttackTypes;

        public AttackTargetState GetAttackTargetState => GetAttackTarget;
        [SerializeField] AttackTargetState GetAttackTarget;

        public  void Setup(bool team, NPCLevel level, TeamInfo teamInfo, List<AttackTypeInfo> attackTypeInfos, RetreatCitizen flee)
        {
            isPartofTeam = team;
            _getNPCLevel = level;
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
                    IsLeader = IsLeader,
                    
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
            switch (GetInfluence.Level)
            {
                case NPCLevel.Leader:
                    Leader lead = SpawnedGO.AddComponent<Leader>();
                    lead.influence = GetInfluence;
                    //   SpawnedGO.AddComponent<GridPointSpawner>();
                    break;
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