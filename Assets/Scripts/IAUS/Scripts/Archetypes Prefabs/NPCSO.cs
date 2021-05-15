using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.AI;
using IAUS.NPCSO.Interfaces;
using Global.Component;
using IAUS.ECS2;
using Components.MovementSystem;
using IAUS.ECS2.Component;
using Stats;
using AISenses;
using AISenses.Authoring;
using InfluenceSystem.Component;

namespace IAUS.NPCSO
{
    public abstract class NPCSO : ScriptableObject, INPCBasics
    {

        [SerializeField] uint spawnID;
        public uint SpawnID { get { return spawnID; } }
        [SerializeField] string _getName;
        public string GetName => _getName;

        [SerializeField] GameObject _model;
        public GameObject Model { get { return _model; } }
        [SerializeField] Influence getInfluence;
        public Influence GetInfluence => getInfluence;
        public AITarget Self => GetSelf;
        [SerializeField]AITarget GetSelf;
        public List<AIStates> AIStatesAvailable => states;
        [SerializeField] List<AIStates> states;
        public Patrol GetPatrol => getPatrol;
        [SerializeField] Patrol getPatrol;
        public Wait GetWait => getWait;
        [SerializeField] Wait getWait;
        public Retreat GetRetreat => getRetreat;
        [SerializeField] Retreat getRetreat;
        public Movement AIMove => GetMovement;
        [SerializeField] Movement GetMovement;

        [SerializeField] TypeOfNPC getNPCType;
        public TypeOfNPC GetTypeOfNPC => getNPCType;

        [SerializeField]Vision getVision;
        public Vision GetVision => getVision;
        [SerializeField] Hearing getHearing;
        public Hearing GetHearing => getHearing;

        public void Setup(string Name,GameObject model, TypeOfNPC typeOf, AITarget self, Vision vision, Hearing hearing, Influence influence, List<AIStates> NpcStates, Movement movement
            ,Patrol patrol,Wait wait, Retreat flee
            ) {
            _getName = Name;
            GetSelf = self;
            getInfluence = influence;
            GetMovement = movement;
            states = NpcStates;
            _model = model;
            getRetreat = flee;
            getPatrol = patrol;
            getWait = wait;
            getNPCType = typeOf;
            getHearing = hearing;
            getVision = vision;
        }
        BaseAIAuthoringSO test;
       public GameObject SpawnedGO { get; private set; }
        public virtual void Spawn( Vector3 pos) {
            SpawnedGO = Instantiate(Model, pos, Quaternion.identity);
            SpawnedGO.AddComponent<NavMeshAgent>();
            SpawnedGO.AddComponent<ConvertToEntity>().ConversionMode = ConvertToEntity.Mode.ConvertAndInjectGameObject;
             test = SpawnedGO.AddComponent<BaseAIAuthoringSO>();
            test.Self = Self;
            test.movement = AIMove;
            foreach (AIStates state in AIStatesAvailable) {
                switch (state) {
                    case AIStates.Patrol:
                        test.AddPatrol = true;
                        test.patrolState = GetPatrol;
                        break;
                    case AIStates.Wait:
                        test.AddWait = true;
                        test.waitState = GetWait;
                        break;
                    case AIStates.Retreat:
                        test.AddRetreat = true;
                        test.retreatState = GetRetreat;
                        break;
                }        
                   
                }
            if (test.AddPatrol)
                SpawnedGO.AddComponent<WaypointCreation>();
         
            AISensesAuthoring Senses = SpawnedGO.AddComponent<AISensesAuthoring>();
            Senses.Vision = true;
            Senses.VisionData = GetVision;
            Senses.HearingData = GetHearing;

        }


    }
}
