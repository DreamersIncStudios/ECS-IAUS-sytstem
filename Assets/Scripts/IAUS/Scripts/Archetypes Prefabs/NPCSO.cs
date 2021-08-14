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
using DreamersInc.InflunceMapSystem;

namespace IAUS.NPCSO
{
    public class NPCSO : ScriptableObject, INPCBasics
    {

        [SerializeField] uint spawnID;
        public uint SpawnID { get { return spawnID; } }
        [SerializeField] string _getName;
        public string GetName => _getName;

        [SerializeField] GameObject _model;
        public GameObject Model { get { return _model; } }
       public  InfluenceComponent GetInflunce=> throw new System.NotImplementedException();

        [SerializeField] InfluenceComponent getInfluence;

        public AITarget Self => GetSelf;
        [SerializeField]AITarget GetSelf;
        public List<AIStates> AIStatesAvailable => states;
        [SerializeField] List<AIStates> states;
        public Patrol GetPatrol => getPatrol;
        [SerializeField] Patrol getPatrol;
        public Wait GetWait => getWait;
        [SerializeField] Wait getWait;

        public Movement AIMove => GetMovement;
        [SerializeField] Movement GetMovement;

        [SerializeField] TypeOfNPC getNPCType;
        public TypeOfNPC GetTypeOfNPC => getNPCType;

        [SerializeField]Vision getVision;
        public Vision GetVision => getVision;


        public void Setup(string Name,GameObject model, TypeOfNPC typeOf, AITarget self, Vision vision, List<AIStates> NpcStates, Movement movement
            ,Patrol patrol,Wait wait
            ) {
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
       public BaseAIAuthoringSO AIAuthoring;
       public GameObject SpawnedGO { get; private set; }


        public virtual void Spawn( Vector3 pos) {
            SpawnedGO = Instantiate(Model, pos, Quaternion.identity);
            SpawnedGO.AddComponent<NavMeshAgent>();
            SpawnedGO.AddComponent<ConvertToEntity>().ConversionMode = ConvertToEntity.Mode.ConvertAndInjectGameObject;
             AIAuthoring = SpawnedGO.AddComponent<BaseAIAuthoringSO>();
            AIAuthoring.Self = Self;
            AIAuthoring.movement = AIMove;
            foreach (AIStates state in AIStatesAvailable) {
                switch (state) {
                    case AIStates.Patrol:
                        AIAuthoring.AddPatrol = true;
                        AIAuthoring.patrolState = GetPatrol;
                        break;
                    case AIStates.Wait:
                        AIAuthoring.AddWait = true;
                        AIAuthoring.waitState = GetWait;
                        break;
                }        
                   
                }
            if (AIAuthoring.AddPatrol)
                SpawnedGO.AddComponent<WaypointCreation>();
         
            AISensesAuthoring Senses = SpawnedGO.AddComponent<AISensesAuthoring>();
            Senses.Vision = true;
            Senses.VisionData = GetVision;
            Senses.Hearing = true;
            Senses.HearingData = new Hearing();
            

        }


    }
}
