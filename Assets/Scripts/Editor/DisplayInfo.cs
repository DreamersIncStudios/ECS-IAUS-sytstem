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
    public sealed partial class NPCEditor : EditorWindow
    {

        void DisplayBasicInfo() {

            EditorGUILayout.BeginVertical("Box", GUILayout.Width(500));
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            Name = EditorGUILayout.TextField("Name", Name);
           // scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical("Button");
            createRandomCharacter = EditorGUILayout.Foldout(createRandomCharacter, "Create RNG GO To Be Implement Later");

            if (!createRandomCharacter)
            {
                GetModel = (GameObject)EditorGUILayout.ObjectField("Select Model", GetModel, typeof(GameObject), false);
            }
            else
            {
                GUILayout.Label("To Be Implemented at a later date......");
            }
            GetTypeOfNPC = (TypeOfNPC)EditorGUILayout.EnumPopup("NPC Type", GetTypeOfNPC);
            GetTargetType = (TargetType)EditorGUILayout.EnumPopup("AI Type", GetTargetType);
            GetInfluence = SetupInfluence();
            if (GetTargetType == TargetType.Character)
                GetMove = SetupMove(GetMove);
            EditorGUILayout.EndVertical();
         //   EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();


        }
        void DisplayStatInfo() { 
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(500));
            GUILayout.Label("Character Stats Info", EditorStyles.boldLabel);


            EditorGUILayout.EndVertical();

        }
        void DisplayDetectionInfo() {
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(500));
            GUILayout.Label("Enemy Detection Settings", EditorStyles.boldLabel);

            GetVision = SetupVision();
            GetHearing = SetupHearing();
            EditorGUILayout.EndVertical();
        }
        void DisplayAIStates() {
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(500));
            GUILayout.Label("Character Stats Info", EditorStyles.boldLabel);
            AiStateInt = GUILayout.Toolbar(AiStateInt, AIStatesTab);
            switch (AiStateInt) {
                case 0:
                    GetPatrol = SetupPatrol(GetPatrol);
                    break;
                case 1:
                    GetWait = SetupWait(GetWait);
                    break;
                case 2:
                    GetAttacks = SetupAttacks();
                    break;
                case 3:
                    GetRetreat = SetupFlee(GetRetreat);
                    break;
            //    case 0:
            //        break;
            //    case 0:
            //        break;
            }

            EditorGUILayout.EndVertical();
        }
    }
}