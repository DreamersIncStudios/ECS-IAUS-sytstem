using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using IAUS.ECS2.Component;
using IAUS.ECS2;
using Global.Component;
using SpawnerSystem.Editors;
using IAUS.SO.Interfaces;
using Components.MovementSystem;
namespace IAUS.SO.editor
{
    public sealed partial class NPCEditor : EditorWindow {

        void DisplayNPCSOForEditing(NPCSO SO) {
            editorState = EditorState.EditExisting;
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

            GetWait = SO.GetWait;
        }
        NPCSO GetNPCSO;
        void DisplayListOfExistingSO() {
            EditorGUILayout.BeginVertical("Box");
            if (!NPCSODatabase.IsLoaded)
                NPCSODatabase.LoadDatabase();
            foreach (NPCSO SO in NPCSODatabase.NPCs)
            {
                EditorGUILayout.BeginHorizontal();
                //add names
                if (GUILayout.Button("testing"))
                    GetNPCSO = SO;
                    DisplayNPCSOForEditing(SO);
                // delete SO
                GUILayout.Button("X", GUILayout.Width(20));
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Create New SO"))
            {
                editorState = EditorState.CreateNew;
                //reset to start values;
            }

            EditorGUILayout.EndVertical();
        }
        void SaveChanges() {

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

            GetNPCSO.Setup(GetModel, new AITarget() { Type = GetTargetType }, StatesToAdd, GetMove,
                GetPatrol, GetWait, GetRetreat
                );
            SetStartValues();

        }


        enum EditorState {
           CreateNew, EditExisting
        }
    }
}