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
    public class EnemyNPCSO : NPCSO, IEnemyNPC
    {
        public bool IsPartOfTeam => isPartofTeam;
        [SerializeField] bool isPartofTeam = false;
        public NPCLevel GetNPCLevel => _getNPCLevel;
        [SerializeField] NPCLevel _getNPCLevel;
        public bool IsLeader => (int)_getNPCLevel > 2;
        public TeamInfo GetTeamInfo => getTeamInfo;
        [SerializeField] TeamInfo getTeamInfo;
        public  void Setup(bool team, NPCLevel level, TeamInfo teamInfo)
        {
            isPartofTeam = team;
            _getNPCLevel = level;
            getTeamInfo = teamInfo;
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
            switch (GetInfluence.Level) 
            {
                case InfluenceSystem.Component.NPCLevel.Leader:
                    Leader lead =SpawnedGO.AddComponent<Leader>();
                    lead.influence = GetInfluence;
                 //   SpawnedGO.AddComponent<GridPointSpawner>();
                    break;
            }
        }

    }
}