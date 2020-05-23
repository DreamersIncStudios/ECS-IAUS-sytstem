using UnityEngine;
using UnityEditor;
using Unity.Entities;
using ProjectRebirth.Bestiary.Interfaces;
using IAUS.ECS2;
using InfluenceMap.Factions;
using System.Collections.Generic;

namespace ProjectRebirth.Bestiary
{


    public class EnemyNPC : NPCBase, AIDriven, iPatrol, iWait, iFaction
    {
        [SerializeField] ActiveAIStates _activeAIStates;
        [SerializeField] ConsiderationData health = new ConsiderationData() {
            responseType = ResponseType.Logistic,
            M = 50,
            K = -1,
            B = .91f,
            C = .35f,
            Inverse = false
        };
        [SerializeField] ConsiderationData _distanceToTarget = new ConsiderationData() {
            responseType = ResponseType.Logistic,
            M = 50,
            K = -.95f,
            B = .935f,
            C = .35f,
            Inverse = false

        };
        [SerializeField] ConsiderationData _waitTimer = new ConsiderationData()
        {
            responseType = ResponseType.Logistic,
            M = 50,
            K = -1f,
            B = .91f,
            C = .2f,
            Inverse = false

        };
        [SerializeField] float _bufferZone = .5f;
        [SerializeField] float _timeToWait = 20f;
        [SerializeField] float _resetTimer = 5.5f;
  
        [SerializeField] Attackable _factionData;
        [SerializeField] bool _canAttack;

        

        public ActiveAIStates activeAIStates { get { return _activeAIStates; } }
        public ConsiderationData Health { get { return health; } }

        public ConsiderationData DistanceToTarget { get {return _distanceToTarget; } }

        public float BufferZone { get { return _bufferZone; } }

        public float ResetTime { get { return _resetTimer; } }

        public float TimeToWait { get { return _timeToWait; } }

        public ConsiderationData WaitTimer { get { return _waitTimer; } }

        public Attackable FactionData { get { return _factionData; } }

        public bool AbleToAttack { get { return _canAttack; } }


        public override void Spawn(Vector3 pos)
        {
            GameObject prefab = Instantiate(GO);
           
            authoringtest tester = prefab.AddComponent<authoringtest>();
            tester.ActiveAIStates = activeAIStates;
            if (activeAIStates.Patrol)
            {
                Patrol patrol = new Patrol()
                {
                    Health = health,
                    DistanceToTarget = DistanceToTarget,
                    _resetTimer = ResetTime,
                    BufferZone = BufferZone
                };
                tester.patrol = patrol;
            }
            if (activeAIStates.Attack) {
                tester.attack = FactionData;
            }
         
        }
    }



    public class authoringtest : MonoBehaviour, IConvertGameObjectToEntity
    {
        public authoringtest() { }
        public authoringtest( ActiveAIStates States, Attackable FactionData,Patrol patrolData)
        {
            attack = FactionData;
            patrolData = patrol;
            ActiveAIStates = States;
        }
        public Attackable attack;
        public Patrol patrol;
        public List<Transform> PatrolPoints;
        public ActiveAIStates ActiveAIStates;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if(ActiveAIStates.Attack)
                dstManager.AddComponentData(entity, attack);
            if (ActiveAIStates.Patrol)
            {
                dstManager.AddComponentData(entity, patrol);
                dstManager.AddBuffer<PatrolBuffer>(entity);
            }
        }

    }
}