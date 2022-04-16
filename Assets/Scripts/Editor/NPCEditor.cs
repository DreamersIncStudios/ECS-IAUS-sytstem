using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using IAUS.ECS.Component;
using IAUS.ECS;
using IAUS.ECS.Consideration;
using Global.Component;
using Dreamers.Global;
using IAUS.NPCScriptableObj.Interfaces;
using Components.MovementSystem;
using DreamersInc.InflunceMapSystem;
namespace IAUS.NPCScriptableObj.editor
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

        string[] menuOptions = new string[] { "Basic", "Level/Stats", "Detection", "AI States", };
        string[] AIStatesTab = new string[] { "Patrol", "Wait", "Attack", "Retreat" };
        int menuInt = 0;
        int AiStateInt = 0;
        public void Awake()
        {
            SetStartValues();
           
        }

        bool[] showBtn = new bool[System.Enum.GetNames(typeof(AIStates)).Length];
        EditorState editorState = EditorState.CreateNew;
        TargetType GetTargetType;
        MovementBuilderData GetPatrol;

        WaitBuilderData GetWait;
        RetreatCitizen GetRetreat;
        GameObject GetModel;
        bool createRandomCharacter = false;
        TypeOfNPC GetTypeOfNPC;
        Vector2 scrollPos;
        string Name;
        void OnGUI()
        {
            menuInt = GUILayout.Toolbar(menuInt, menuOptions);

            EditorGUILayout.BeginHorizontal();
            DisplayListOfExistingSO();
            switch (menuInt) {
                case 0:
                    DisplayBasicInfo();
                    break;
                case 1:
                    DisplayStatInfo();
                    break;
                case 2:
                    DisplayDetectionInfo();
                    break;
                case 3:
                    DisplayAIStates();
                    break;
            
            }
            //GetTeam = SetupEnemy();



            EditorGUILayout.EndVertical();

            // add a switch here
            EditorGUILayout.BeginHorizontal("Box");
            switch (editorState)
            {
                case EditorState.CreateNew:

                    if (GUILayout.Button("Submit"))
                    {
                        CreateSO("Assets/Resources/NPC");
                        Repaint();

                    }
                    break;
                case EditorState.EditExisting:

                    if (GUILayout.Button("Update"))
                    {
                        SaveChangesToNPCSO();
                        Repaint();


                    }
                    if (GUILayout.Button("Create New SO"))
                    {
                        CreateSO("Assets/Resources/NPC SO AI");
                        Repaint();

                    }
                    break;
            }
            if (GUILayout.Button("Clear"))
            {
                // add nodal window to verfiy 
                SetStartValues();
                Repaint();
            }
            EditorGUILayout.EndHorizontal();

        }


        bool PatrolDistance = false;
        bool PatrolHealthRatio = false;
        MovementBuilderData SetupPatrol(MovementBuilderData state)
        {
            showBtn[(int)AIStates.Patrol] = EditorGUILayout.BeginFoldoutHeaderGroup(showBtn[(int)AIStates.Patrol], "Patrol");

            if (showBtn[(int)AIStates.Patrol])
            {
                state.BufferZone = EditorGUILayout.FloatField("Buffer Zone", state.BufferZone);
                state.CoolDownTime = EditorGUILayout.FloatField("Cool Down Time", state.CoolDownTime);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            return state;
        }
        bool WaitTime = false;
        bool WaitHealth = false;
        WaitBuilderData SetupWait(WaitBuilderData state)
        {
            showBtn[(int)AIStates.Wait] = EditorGUILayout.BeginFoldoutHeaderGroup(showBtn[(int)AIStates.Wait], "Wait at Location");
            if (showBtn[(int)AIStates.Wait])
            {
                state.StartTime = EditorGUILayout.FloatField("Start Time", state.StartTime);
                state.CoolDownTime = EditorGUILayout.FloatField("Cool Down Time", state.CoolDownTime);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            return state;
        }

        bool RetreatDistance, RetreatHealthRatio,AlertResponse;
       RetreatCitizen SetupRetreat(RetreatCitizen state)
        {
            showBtn[(int)AIStates.Retreat] = EditorGUILayout.BeginFoldoutHeaderGroup(showBtn[(int)AIStates.Retreat], "Flee from Target");
            if (showBtn[(int)AIStates.Retreat])
            {
                if (RetreatHealthRatio = EditorGUILayout.Foldout(RetreatHealthRatio, "CharacterHealth"))
                    state.HealthRatio = DisplayConsideration(state.HealthRatio);
                if (AlertResponse = EditorGUILayout.Foldout(RetreatHealthRatio, "Alert Response"))
                    state.ProximityInArea= DisplayConsideration(state.ProximityInArea);

                state._coolDownTime = EditorGUILayout.FloatField("Cool Down Time", state._coolDownTime);
                state.HideTime = EditorGUILayout.FloatField("Hide Time", state.HideTime);



            }

            if (GUILayout.Button("Reset")) {
               state = new RetreatCitizen()
                {
                    HealthRatio = new ConsiderationScoringData() { M = 50, K = -1, B = .91f, C = .2f, responseType = ResponseType.Logistic },
                    ProximityInArea = new ConsiderationScoringData() { M = 50, K = -0.95f, B = .935f, C = .35f, responseType = ResponseType.Logistic },
                    _coolDownTime = 5,
                    HideTime = 30

                };

            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            return state;
        }

        ConsiderationScoringData DisplayConsideration(ConsiderationScoringData data)
        {
            data.Inverse = EditorGUILayout.Toggle("Inverse", data.Inverse);
            data.responseType = (ResponseType)EditorGUILayout.EnumPopup("Response", data.responseType);
            data.M = EditorGUILayout.FloatField("M", data.M);
            data.K = EditorGUILayout.FloatField("K", data.K);
            data.B = EditorGUILayout.FloatField("B", data.B);
            data.C = EditorGUILayout.FloatField("C", data.C);
            return data;
        }
        Movement GetMove;
        Movement SetupMove(Movement move)
        {
            move.MovementSpeed = EditorGUILayout.FloatField("Movement Speed", move.MovementSpeed);
            move.StoppingDistance = EditorGUILayout.FloatField("Stopping Distance", move.StoppingDistance);
            move.Acceleration = EditorGUILayout.FloatField("Acceleration", move.Acceleration);
            move.MaxInfluenceAtPoint = EditorGUILayout.IntField(" Max Influence at location allowed", move.MaxInfluenceAtPoint);


            return move;
        }


        void CreateSO(string Path)
        {

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
                        case AIStates.Retreat:
                            StatesToAdd.Add(AIStates.Retreat);
                            break;
                    }

                }
            }
            //Create SO base of NPC Type
            switch (GetTypeOfNPC)
            {
                case TypeOfNPC.Neurtal:
                    ScriptableObjectUtility.CreateAsset<NPCSO>(Path, Name, out NPCSO SO);
                    SO.Setup(Name, GetModel, GetTypeOfNPC, new AITarget() { Type = GetTargetType }, GetVision,  StatesToAdd, GetMove,
                        GetPatrol, GetWait
                        );
                    EditorUtility.SetDirty(SO);
                    break;
                case TypeOfNPC.Friendly:
                    break;
                case TypeOfNPC.Enemy:
                    ScriptableObjectUtility.CreateAsset<EnemyNPCSO>(Path, Name, out EnemyNPCSO enemyNPCSO);
                    enemyNPCSO.Setup(Name, GetModel, GetTypeOfNPC, new AITarget() { Type = GetTargetType }, GetVision,  StatesToAdd, GetMove,
                        GetPatrol, GetWait, GetTeam.IsLeader, GetTeamInfo, GetAttacks, GetRetreat,GetInfluence
                        );
                    EditorUtility.SetDirty(enemyNPCSO);

                    break;
            }


            SetStartValues();
        }

        public void SetStartValues()
        {
            Name = null;
            GetModel = null;
            GetTypeOfNPC = TypeOfNPC.Neurtal;
            GetTargetType = new TargetType();
            GetPatrol = new  MovementBuilderData()
            {
                BufferZone = .75f,
                CoolDownTime = 5

            };
            GetRetreat = new RetreatCitizen() {
                HealthRatio = new ConsiderationScoringData() { M = 50, K = -1, B = .91f, C = .2f, responseType = ResponseType.Logistic },
                ProximityInArea = new ConsiderationScoringData() { M = 50, K = -0.95f, B = .935f, C = .35f, responseType = ResponseType.Logistic },
                _coolDownTime = 5,
                HideTime = 30

            };
            GetWait = new WaitBuilderData()
            {
                StartTime = 1,
                CoolDownTime = 5
            };
            GetVision = new AISenses.Vision()
            {
                ViewAngle = 120,
                viewRadius = 35,
                EngageRadius = 10,
            };

            GetInfluence = new InfluenceComponent();
            GetAttacks = new List<AttackTypeInfo>();
            editorState = EditorState.CreateNew;
        }

    }

}