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
        Patrol GetPatrol;

        Wait GetWait;
        Retreat GetRetreat;
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
                        CreateSO("Assets/Resources/NPC SO AI");
                    }
                    break;
                case EditorState.EditExisting:

                    if (GUILayout.Button("Update"))
                    {
                        SaveChangesToNPCSO();
                    }
                    if (GUILayout.Button("Create New SO"))
                    {
                        CreateSO("Assets/Resources/NPC SO AI");
                    }
                    break;
            }
            if (GUILayout.Button("Clear"))
            {
                // add nodal window to verfiy 
                SetStartValues();
            }
            EditorGUILayout.EndHorizontal();

        }


        bool PatrolDistance = false;
        bool PatrolHealthRatio = false;
        Patrol SetupPatrol(Patrol state)
        {
            showBtn[(int)AIStates.Patrol] = EditorGUILayout.BeginFoldoutHeaderGroup(showBtn[(int)AIStates.Patrol], "Patrol");

            if (showBtn[(int)AIStates.Patrol])
            {
                if (PatrolDistance = EditorGUILayout.Foldout(PatrolDistance, "Distance To Consideration"))
                    state.DistanceToPoint = DisplayConsideration(state.DistanceToPoint);
                if (PatrolHealthRatio = EditorGUILayout.Foldout(PatrolHealthRatio, "CharacterHealth"))
                    state.HealthRatio = DisplayConsideration(state.HealthRatio);
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

        bool RetreatDistance, RetreatHealthRatio;
        Retreat SetupFlee(Retreat state)
        {
            showBtn[(int)AIStates.Retreat] = EditorGUILayout.BeginFoldoutHeaderGroup(showBtn[(int)AIStates.Retreat], "Flee from Target");
            if (showBtn[(int)AIStates.Retreat])
            {
                if (RetreatDistance= EditorGUILayout.Foldout(RetreatDistance, "Distance To Consideration"))
                    state.DistanceToSafe = DisplayConsideration(state.DistanceToSafe);
                if (RetreatHealthRatio = EditorGUILayout.Foldout(RetreatHealthRatio, "CharacterHealth"))
                    state.HealthRatio = DisplayConsideration(state.HealthRatio);

                state.BufferZone = EditorGUILayout.FloatField("Buffer Zone", state.BufferZone);
                state._coolDownTime = EditorGUILayout.FloatField("Cool Down Time", state._coolDownTime);
                state.HideTime = EditorGUILayout.FloatField("Cool Down Time", state.HideTime);
                state.EscapeRange = EditorGUILayout.IntField("Cool Down Time", state.EscapeRange);



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
            move.MaxInfluenceAtPoint = EditorGUILayout.IntField("Max Influence???", move.MaxInfluenceAtPoint);


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

                    }

                }
            }
            //Create SO base of NPC Type
            switch (GetTypeOfNPC)
            {
                case TypeOfNPC.Neurtal:
                    break;
                case TypeOfNPC.Friendly:
                    break;
                case TypeOfNPC.Enemy:
                    break;
            }
            switch (GetTypeOfNPC)
            {
                case TypeOfNPC.Neurtal:
                    ScriptableObjectUtility.CreateAsset<NPCSO>(Path, out NPCSO SO);
                    SO.Setup(Name, GetModel, GetTypeOfNPC, new AITarget() { Type = GetTargetType }, GetVision, GetHearing, GetInfluence, StatesToAdd, GetMove,
                        GetPatrol, GetWait, GetRetreat
                        );
                    break;
                case TypeOfNPC.Friendly:
                    break;
                case TypeOfNPC.Enemy:
                    ScriptableObjectUtility.CreateAsset<EnemyNPCSO>(Path, out EnemyNPCSO enemyNPCSO);
                    enemyNPCSO.Setup(Name, GetModel, GetTypeOfNPC, new AITarget() { Type = GetTargetType }, GetVision, GetHearing, GetInfluence, StatesToAdd, GetMove,
                        GetPatrol, GetWait, GetRetreat
                        );
                    enemyNPCSO.Setup(GetTeam.IsLeader, GetInfluence.Level, GetTeamInfo);
                    break;
            }


            SetStartValues();
        }

        public void SetStartValues()
        {
            GetModel = null;
            GetTypeOfNPC = TypeOfNPC.Neurtal;
            GetTargetType = new TargetType();
            GetPatrol = new Patrol()
            {
                BufferZone = .75f,
                DistanceToPoint = new ConsiderationScoringData() { M = 50, K = -0.95f, B = .935f, C = .35f, responseType = ResponseType.Logistic },
                HealthRatio = new ConsiderationScoringData() { M = 50, K = -1, B = .91f, C = .2f, responseType = ResponseType.Logistic },
                _coolDownTime = 5

            };
            GetRetreat = new Retreat() {
                HealthRatio = new ConsiderationScoringData() { M = 50, K = -1, B = .91f, C = .2f, responseType = ResponseType.Logistic },
                DistanceToSafe = new ConsiderationScoringData() { M = 50, K = -0.95f, B = .935f, C = .35f, responseType = ResponseType.Logistic },
                InfluenceInArea = new ConsiderationScoringData() { M = 50, K = -0.95f, B = .935f, C = .35f, responseType = ResponseType.Logistic },
                BufferZone = .75f,
                _coolDownTime = 5,
                EscapeRange = 25,
                HideTime = 30

            };
            GetWait = new Wait()
            {
                TimeLeft = new ConsiderationScoringData() { M = 50, K = -1, B = .91f, C = .2f, responseType = ResponseType.Logistic, Inverse = false },
                HealthRatio = new ConsiderationScoringData() { M = 50, K = -1, B = .91f, C = .2f, responseType = ResponseType.Logistic, Inverse = false },
                StartTime = 1,
                _coolDownTime = 5
            };
            GetVision = new AISenses.Vision()
            {
                ViewAngle = 120,
                viewRadius = 35,
                EngageRadius = 10,
                Scantimer = 5
            };
            GetHearing = new AISenses.Hearing();
            GetInfluence = new InfluenceSystem.Component.Influence();
            GetAttacks = new List<AttackTypeInfo>();
        }

    }

}