﻿using System.Collections;
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
    public sealed partial class NPCEditor : EditorWindow
    {

        [MenuItem("Window/IAUS NPC Creator")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            NPCEditor window = (NPCEditor)EditorWindow.GetWindow(typeof(NPCEditor));
            window.minSize = new Vector2(600, 800);
            window.Show();
        }
        private void Awake()
        {
            SetStartValues();
        }
        bool[] showBtn = new bool[System.Enum.GetNames(typeof(AIStates)).Length] ;
        EditorState editorState;
        TargetType GetTargetType;
        Patrol GetPatrol;
            
        Wait GetWait;
        Retreat GetRetreat;
        GameObject GetModel;
        bool createRandomCharacter=false;
        Vector2 scrollPos;
        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            DisplayListOfExistingSO();

            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical("Button");
            createRandomCharacter = EditorGUILayout.Foldout(createRandomCharacter, "Create RNG GO To Be Implement Later");
            if (!createRandomCharacter)
                GetModel = (GameObject)EditorGUILayout.ObjectField("Select Model", GetModel, typeof(GameObject), false);

            GetTargetType = (TargetType)EditorGUILayout.EnumPopup("AI Type", GetTargetType);
            switch (GetTargetType)
            {
                case TargetType.Character:
                    GetPatrol = SetupPatrol(GetPatrol);
                    GetWait = SetupWait(GetWait);
                    SetupFlee();
                    break;
            }
            EditorGUILayout.EndVertical();

            if (GetTargetType == TargetType.Character)
                GetMove = SetupMove(GetMove);
            // add a switch here
            EditorGUILayout.BeginHorizontal("Box");
            switch (editorState)
            {
                case EditorState.CreateNew:

                    if (GUILayout.Button("Submit"))
                    {
                        CreateSO("Assets/Resources/NPC SO AI");
                    }
                        break;
                case EditorState.EditExisting:

                    if (GUILayout.Button("Update"))
                    {
                        SaveChanges();
                    }
                    if (GUILayout.Button("Update"))
                    {
                        CreateSO("Assets/Resources/NPC SO AI");
                    }
                    break;
            }
            if (GUILayout.Button("Clear"))
            {
            // add nodal window to verfiy 
                SetStartValues();
                editorState = EditorState.CreateNew;
            }


            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        bool PatrolDistance = false;
        bool PatrolHealthRatio = false;
        Patrol SetupPatrol( Patrol state)
        {
            showBtn[(int)AIStates.Patrol] = EditorGUILayout.BeginFoldoutHeaderGroup(showBtn[(int)AIStates.Patrol], "Patrol");

            if (showBtn[(int)AIStates.Patrol])
            {
                if(PatrolDistance = EditorGUILayout.Foldout(PatrolDistance, "Distance To Consideration"))
                   state.DistanceToPoint = DisplayConsideration(state.DistanceToPoint);
                if(PatrolHealthRatio = EditorGUILayout.Foldout(PatrolHealthRatio, "CharacterHealth"))
                    state.HealthRatio = DisplayConsideration( state.HealthRatio);
                state.BufferZone = EditorGUILayout.FloatField("Buffer Zone", state.BufferZone);
                state._coolDownTime = EditorGUILayout.FloatField("Cool Down Time", state._coolDownTime);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            return state;
        }
        bool WaitTime = false;
        bool WaitHealth = false;
        Wait SetupWait(Wait state)
        {
            showBtn[(int)AIStates.Wait] = EditorGUILayout.BeginFoldoutHeaderGroup(showBtn[(int)AIStates.Wait], "Wait at Location");
            if (showBtn[(int)AIStates.Wait])
            {
                if (WaitTime = EditorGUILayout.Foldout(WaitTime, "Time Left"))
                    state.TimeLeft = DisplayConsideration(state.TimeLeft);

                if (WaitHealth = EditorGUILayout.Foldout(WaitHealth, "Time Left"))
                    state.HealthRatio = DisplayConsideration(state.HealthRatio);
                state.StartTime = EditorGUILayout.FloatField("Start Time", state.StartTime);
                state._coolDownTime = EditorGUILayout.FloatField("Cool Down Time", state._coolDownTime);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            return state;
        }
        void SetupFlee()
        {
            showBtn[(int)AIStates.Retreat] = EditorGUILayout.BeginFoldoutHeaderGroup(showBtn[(int)AIStates.Retreat], "Flee from Target");
            if (showBtn[(int)AIStates.Retreat])
            {
                GUILayout.Button("Submit");
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

       ConsiderationScoringData DisplayConsideration( ConsiderationScoringData data) {
            data.Inverse = EditorGUILayout.Toggle("Inverse",data.Inverse);
            data.responseType = (ResponseType)EditorGUILayout.EnumPopup("Response", data.responseType);
            data.M = EditorGUILayout.FloatField("M", data.M);
            data.K = EditorGUILayout.FloatField("K", data.K);
            data.B = EditorGUILayout.FloatField("B", data.B );
            data.C = EditorGUILayout.FloatField("C", data.C);
            return data;
        }
        Movement GetMove;
        Movement SetupMove(Movement move) {
            move.MovementSpeed = EditorGUILayout.FloatField("Movement Speed", move.MovementSpeed);
            move.StoppingDistance = EditorGUILayout.FloatField("Stopping Distance", move.StoppingDistance);
            move.Acceleration = EditorGUILayout.FloatField("Acceleration", move.Acceleration);
            move.MaxInfluenceAtPoint = EditorGUILayout.IntField("Max Influence???", move.MaxInfluenceAtPoint);


            return move;
        }
        

        void CreateSO(string Path) {

            List<AIStates> StatesToAdd= new List<AIStates>();
            for (int i = 0; i < showBtn.Length; i++)
            {
                if (showBtn[i]) {
                    switch ((AIStates)i) {
                        case AIStates.Patrol:
                            StatesToAdd.Add( AIStates.Patrol);
                            break;
                        case AIStates.Wait:
                            StatesToAdd.Add(AIStates.Wait);
                            break;

                    }
                
                }
            }

            ScriptableObjectUtility.CreateAsset<NPCSO>(Path, out NPCSO SO);
            SO.Setup(GetModel,new AITarget() { Type = GetTargetType },StatesToAdd, GetMove,
                GetPatrol,GetWait,GetRetreat
                );
            SetStartValues();
        }

       public void SetStartValues() {
            GetModel = null;
            GetTargetType = new TargetType();
            GetPatrol = new Patrol()
            {
                BufferZone = .75f,
                DistanceToPoint = new ConsiderationScoringData() { M = 50, K = -0.95f, B = .935f, C = .35f, responseType = ResponseType.Logistic },
                HealthRatio = new ConsiderationScoringData() { M = 50, K = -1, B = .91f, C = .2f, responseType = ResponseType.Logistic },
                _coolDownTime = 5

            };
            GetRetreat = new Retreat() { };
            GetWait = new Wait() { 
                TimeLeft = new ConsiderationScoringData() { M=50, K=-1, B=.91f,C= .2f, responseType = ResponseType.Logistic, Inverse =false},
                HealthRatio =  new ConsiderationScoringData() { M = 50, K = -1, B = .91f, C = .2f, responseType = ResponseType.Logistic, Inverse = false },
                StartTime =1,
                _coolDownTime =5
            };

        }

    }
}