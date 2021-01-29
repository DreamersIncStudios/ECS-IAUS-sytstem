using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.AI;
using IAUS.SO.Interfaces;
using Global.Component;
using IAUS.ECS2;
using Components.MovementSystem;
using IAUS.ECS2.Component;
using Stats;
namespace IAUS.SO
{
    public class NPCSO : ScriptableObject, INPCBasics
    {
        [SerializeField] uint spawnID;
        public uint SpawnID { get { return spawnID; } }
        [SerializeField] GameObject _model;
        public GameObject Model { get { return _model; } }
        
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
        [SerializeField] AISenses.Authoring.AISensesAuthoring getAISenses;
        public AISenses.Authoring.AISensesAuthoring GetAISenses => getAISenses;
        public TypeOfNPC GetTypeOfNPC => getNPCType;
        public void Setup(GameObject model, TypeOfNPC typeOf, AITarget self, List<AIStates> stateScorers, Movement movement
            ,Patrol patrol,Wait wait, Retreat flee
            ) {
            GetSelf = self;
            GetMovement = movement;
            states = stateScorers;
            _model = model;
            getRetreat = flee;
            getPatrol = patrol;
            getWait = wait;
            getNPCType = typeOf;
        }
        BaseAIAuthoringSO test;
        public void Spawn( Vector3 pos) {
            GameObject go = Instantiate(Model, pos, Quaternion.identity);
            go.AddComponent<NavMeshAgent>();
            go.AddComponent<ConvertToEntity>().ConversionMode = ConvertToEntity.Mode.ConvertAndInjectGameObject;
             test = go.AddComponent<BaseAIAuthoringSO>();
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
                go.AddComponent<WaypointCreation>();
            if (Self.Type == TargetType.Character)
                go.AddComponent<EnemyCharacter>();
        }


    }
}
