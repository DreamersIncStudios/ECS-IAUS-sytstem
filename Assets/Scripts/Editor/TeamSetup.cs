using System.Collections;
using System.Collections.Generic;
using InfluenceSystem.Component;
using UnityEditor;
using Dreamers.SquadSystem;
using IAUS.NPCSO.Interfaces;
namespace IAUS.NPCSO.editor
{
    public sealed partial class NPCEditor : EditorWindow
    {
        Team GetTeam;
        TeamInfo GetTeamInfo;
        Team SetupEnemy() {
            if (GetInfluence.Level == NPCLevel.Grunt)
                GetTeam.IsLeader = false;
            else
            {
                GetTeam.IsLeader = EditorGUILayout.Toggle("Is Squad Leader", GetTeam.IsLeader);
            }
            if (GetTeam.IsLeader)
            {
                GetTeamInfo.MaxTeamSize = (uint)EditorGUILayout.IntSlider("Max Team Size", (int)GetTeamInfo.MaxTeamSize, 0, 20);
            }
            else
                GetTeamInfo.MaxTeamSize = 0;
            return GetTeam; 
        }

    }
}