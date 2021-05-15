using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using IAUS.ECS2.Component;
using IAUS.ECS2;
using Global.Component;
using SpawnerSystem.Editors;
using IAUS.NPCSO.Interfaces;
using Components.MovementSystem;
namespace IAUS.NPCSO.editor
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
            GetRetreat = SO.GetRetreat;
            GetTargetType = SO.Self.Type;
            Name = SO.GetName;
            GetWait = SO.GetWait;
            GetInfluence = SO.GetInfluence;
        }

        NPCSO GetNPCSO;
        void DisplayListOfExistingSO() {
            EditorGUILayout.BeginVertical("Box");
            //if (!NPCSODatabase.IsLoaded)
                NPCSODatabase.LoadDatabaseForce();
            foreach (INPCBasics SO in NPCSODatabase.NPCs)
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

            GetNPCSO.Setup( Name, GetModel, GetTypeOfNPC,new AITarget() { Type = GetTargetType }, GetVision, GetHearing, GetInfluence, StatesToAdd, GetMove,
                GetPatrol, GetWait, GetRetreat
                );
            SetStartValues();

        }


        enum EditorState {
           CreateNew, EditExisting
        }
    }
}