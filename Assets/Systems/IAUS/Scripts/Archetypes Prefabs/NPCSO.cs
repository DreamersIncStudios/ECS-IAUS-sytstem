using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.AI;
using IAUS.NPCScriptableObj.Interfaces;
using Global.Component;
using IAUS.ECS;
using Components.MovementSystem;
using IAUS.ECS.Component;
using Stats;
using AISenses;
using AISenses.Authoring;
using System;
using System.Threading.Tasks;

namespace IAUS.NPCScriptableObj
{
    public class NPCSO : ScriptableObject, INPCBasics
    {

        [SerializeField] uint spawnID;
        public uint SpawnID { get { return spawnID; } }
        [SerializeField] string _getName;
        public string GetName => string.IsNullOrEmpty(_getName) ? NPCUtility.GetNameFile() : _getName;
        public uint GetLevel { get { return level; } }
        [SerializeField] uint level;

        [SerializeField] GameObject _model;
        public GameObject Model { get { return _model; } }


        public AITarget Self => GetSelf;
        [SerializeField] AITarget GetSelf;
        public List<AIStates> AIStatesAvailable => states;
        [SerializeField] List<AIStates> states;
        public PMovementBuilderData GetPatrol => getPatrol;
        [SerializeField] PMovementBuilderData getPatrol;
        public WaitBuilderData GetWait => getWait;
        [SerializeField] WaitBuilderData getWait;

        public Movement AIMove => GetMovement;
        [SerializeField] Movement GetMovement;

        [SerializeField] TypeOfNPC getNPCType;
        public TypeOfNPC GetTypeOfNPC => getNPCType;

        [SerializeField] Vision getVision;
        public Vision GetVision => getVision;

        public virtual void Setup(string Name, GameObject model, TypeOfNPC typeOf, AITarget self, Vision vision, List<AIStates> NpcStates, Movement movement
            , PMovementBuilderData patrol, WaitBuilderData wait
            )
        {
            _getName = Name;
            GetSelf = self;
            GetMovement = movement;
            states = NpcStates;
            _model = model;
            getPatrol = patrol;
            getWait = wait;
            getNPCType = typeOf;
            getVision = vision;
        }
        [HideInInspector] public BaseAIAuthoringSO AIAuthoring;
        public GameObject SpawnedGO { get; private set; }

        public virtual async void Spawn(Vector3 pos)
        {
            SpawnedGO = Instantiate(Model, pos, Quaternion.identity);
            if (!SpawnedGO.GetComponent<NavMeshAgent>())
            {
                SpawnedGO.AddComponent<NavMeshAgent>();
            }
            SpawnedGO.AddComponent<ConvertToEntity>().ConversionMode = ConvertToEntity.Mode.ConvertAndInjectGameObject;
            AIAuthoring = SpawnedGO.AddComponent<BaseAIAuthoringSO>();
            AIAuthoring.Self = Self;
            //AIAuthoring.faction = getFaction;
            AIAuthoring.movement = AIMove;
            AISensesAuthoring Senses = SpawnedGO.AddComponent<AISensesAuthoring>();
            Senses.Vision = true;
            Senses.VisionData = GetVision;

            foreach (AIStates state in AIStatesAvailable)
            {
                switch (state)
                {
                    case AIStates.Patrol:
                        AIAuthoring.AddPatrol = true;
                        AIAuthoring.buildMovement = GetPatrol;
                        var adder = SpawnedGO.AddComponent<WaypointCreation>();
                        await Task.Delay(TimeSpan.FromSeconds(2));
                        adder.CreateWaypoints(GetPatrol.Range, GetPatrol.NumberOfStops, false);
                        break;
                    case AIStates.Wait:
                        AIAuthoring.AddWait = true;
                        AIAuthoring.waitBuilder = GetWait;
                        break;
                }

            }


            AIAuthoring.SetupSystem();
            //Senses.Hearing = true;
            //Senses.HearingData = new Hearing();

        }
        /// <summary>
        /// Spawn EnemyNPC as a Defender
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="Defender"></param>
        public void Spawn(Vector3 pos, bool Defender)
        {
            if (!Defender)
            {
                Spawn(pos);
            }
            else
            {


            }

        }



    }


    public class NPCUtility
    {
        public static string GetNameFile()
        {
            TextAsset nameFile = Resources.Load("ListOfNames") as TextAsset;
            var lines = nameFile.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            int index = UnityEngine.Random.Range(0, lines.Length - 1);
            return lines[index];
        }
    }
}
