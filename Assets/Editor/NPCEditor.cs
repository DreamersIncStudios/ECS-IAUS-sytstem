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
    public class NPCEditor : EditorWindow
    {

        [MenuItem("Window/IAUS NPC Creator")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            NPCEditor window = (NPCEditor)EditorWindow.GetWindow(typeof(NPCEditor));
            window.minSize = new Vector2(600, 800);
            window.Show();
        }

        bool[] showBtn = new bool[System.Enum.GetNames(typeof(AIStates)).Length] ;
        TargetType GetTargetType;
        Patrol patrol;
        Wait wait;
        void OnGUI()
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("Button");
            GetTargetType = (TargetType)EditorGUILayout.EnumPopup("AI Type",GetTargetType);

            patrol =  SetupPatrol(patrol );
         wait =   SetupWait(wait);
            SetupFlee();
            EditorGUILayout.EndVertical();

            if (GetTargetType == TargetType.Character)
                moveinfo = SetupMove(moveinfo);

            if(GUILayout.Button("Submit")){
                CreateSO("Assets/Resources/NPC SO AI");
            }
                
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
                state.StartTime = EditorGUILayout.FloatField("Buffer Zone", state.StartTime);
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
        Movement moveinfo;
        Movement SetupMove(Movement move) {
            move.MovementSpeed = EditorGUILayout.FloatField("Movement Speed", move.MovementSpeed);
            move.StoppingDistance = EditorGUILayout.FloatField("Stopping Distance", move.StoppingDistance);
            move.Acceleration = EditorGUILayout.FloatField("Acceleration", move.Acceleration);
            move.MaxInfluenceAtPoint = EditorGUILayout.IntField("Max Influence???", move.MaxInfluenceAtPoint);


            return move;
        }
        

        void CreateSO(string Path) {
            List<SOAIState> StatesToAdd= new List<SOAIState>();
            for (int i = 0; i < showBtn.Length; i++)
            {
                if (showBtn[i]) {
                    switch ((AIStates)i) {
                        case AIStates.Patrol:
                            StatesToAdd.Add( new SOAIState() { state= AIStates.Patrol,
                            stateInfo = patrol});
                            break;
                        case AIStates.Wait:
                            StatesToAdd.Add(new SOAIState()
                            {
                                state = AIStates.Wait,
                                stateInfo = wait
                            });
                            break;

                    }
                
                }
            }

            ScriptableObjectUtility.CreateAsset<NPCSO>(Path, out NPCSO SO);
            SO.Setup(new AITarget() { Type = GetTargetType },StatesToAdd );
        
        }
    }
}