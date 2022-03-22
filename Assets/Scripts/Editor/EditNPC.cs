﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using IAUS.ECS.Component;
using IAUS.ECS;
using Global.Component;
using IAUS.NPCScriptableObj.Interfaces;
using Components.MovementSystem;
namespace IAUS.NPCScriptableObj.editor
{
    public sealed partial class NPCEditor : EditorWindow {

        void DisplayNPCSOForEditing(NPCSO SO)
        {
            if (SO.Model)
            {
                GetModel = SO.Model;
                createRandomCharacter = false;
            }
            else
                createRandomCharacter = true;
            GetPatrol = SO.GetPatrol;
            GetTargetType = SO.Self.Type;
            Name = SO.GetName;
            GetWait = SO.GetWait;
            GetMove = SO.AIMove;
            GetVision = SO.GetVision;
            GetTypeOfNPC = SO.GetTypeOfNPC;
            switch (GetTypeOfNPC) {
                case TypeOfNPC.Enemy:
                    EnemyNPCSO enemy = (EnemyNPCSO)GetNPCSO;
                    GetTeamInfo = enemy.GetTeamInfo;
                    //   GetTeam.IsLeader = enemy.IsLeader;
                    GetAttacks = enemy.GetAttackType;
                    GetRetreat = enemy.GetRetreat;
                    GetInfluence = enemy.GetInfluence;

                    break;
                case TypeOfNPC.Friendly:
                    FriendNPCSO friend = (FriendNPCSO)GetNPCSO;
                    GetInfluence = friend.GetInfluence;

                    break;
            }

        }

        NPCSO GetNPCSO;
        void DisplayListOfExistingSO() {
            EditorGUILayout.BeginVertical("Box");
            //if (!NPCSODatabase.IsLoaded)
                EnemyDatabase.LoadDatabaseForce();
            foreach (INPCBasics SO in EnemyDatabase.NPCs)
            {
                EditorGUILayout.BeginHorizontal();
                //add names
                if (GUILayout.Button(SO.GetName)) { 
                   GetNPCSO = (NPCSO)SO;
                    DisplayNPCSOForEditing((NPCSO)SO);
                    editorState = EditorState.EditExisting;
                }
                // delete SO
                GUILayout.Button("X", GUILayout.Width(20));
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Create New SO"))
            {
                editorState = EditorState.CreateNew;
                SetStartValues();
                //reset to start values;
            }

            EditorGUILayout.EndVertical();
        }
        void SaveChangesToNPCSO() {

            List<AIStates> StatesToAdd = new List<AIStates>();
            for (int i = 0; i < showBtn.Length; i++)
            {
                if (showBtn[i])
                {
                    switch ((AIStates)i)
                    {
                        case AIStates.Patrol:
                            StatesToAdd.Add(AIStates.Patrol);
                            break;
                        case AIStates.Wait:
                            StatesToAdd.Add(AIStates.Wait);
                            break;

                    }

                }
            }
            switch (GetTypeOfNPC) {
                case TypeOfNPC.Neurtal:
                    GetNPCSO.Setup(Name, GetModel, GetTypeOfNPC, new AITarget() { Type = GetTargetType }, GetVision, StatesToAdd, GetMove,
    GetPatrol, GetWait
    );
                    EditorUtility.SetDirty(GetNPCSO);
                    break;
                case TypeOfNPC.Enemy:
                    EnemyNPCSO enemy = (EnemyNPCSO)GetNPCSO;
                    enemy.Setup(Name, GetModel, GetTypeOfNPC, new AITarget() { Type = GetTargetType }, GetVision, StatesToAdd, GetMove,
                        GetPatrol, GetWait,GetTeam.IsLeader, GetTeamInfo, GetAttacks, GetRetreat, GetInfluence);
                    EditorUtility.SetDirty(enemy);
                    break;

                    //Implement Friendly
                case TypeOfNPC.Friendly: break;

            }


            if (GetTypeOfNPC == TypeOfNPC.Enemy)
            {

            }
            SetStartValues();
        }


        enum EditorState {
           CreateNew, EditExisting
        }
    }
}