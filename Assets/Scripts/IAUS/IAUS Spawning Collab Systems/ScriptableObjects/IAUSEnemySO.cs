using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpawnerSystem.ScriptableObjects;
using IAUS.ECS2;
using ProjectRebirth.Bestiary.Interfaces;
using Unity.Entities;
using Components.MovementSystem;

namespace IAUS.SpawnerSystem
{

    public class IAUSEnemySO : Enemy, iPatrol
    {
        [Header("Patrol")]
        [SerializeField] ActiveAIStates NPCAIStates;
        [SerializeField] ConsiderationData health = new ConsiderationData
        {
            responseType = ResponseType.Logistic,
            M = 50,
            K = -1,
            B = .91f,
            C = .35f
        };
        [SerializeField]
        ConsiderationData distanceToPatrolTarget = new ConsiderationData()
        {
            responseType = ResponseType.Logistic,
            M = 50,
            K = -.95f,
            B = .935f,
            C = .35f
        };
        [SerializeField] float bufferZone = .5f;

        [Header("Movement")]
        [SerializeField] Movement move;
        float resetTime = 5;// decide if this standardized or not
        public ConsiderationData Health { get { return health; } }

        public ConsiderationData DistanceToTarget { get { return distanceToPatrolTarget; } }

        public float BufferZone{ get { return bufferZone; } }

        public float ResetTime { get { return resetTime; } }

        public GameObject Spawn(Vector3 Position, List<GameObject> Points) {
            GameObject NPC = Spawn(Position);
                   IAUSAuthoring AIAuthor = NPC.AddComponent<IAUSAuthoring>();
            AIAuthor.StatesToAdd = NPCAIStates;
            if (NPCAIStates.Patrol)
            {
                AIAuthor.PatrolState = new Patrol()
                {
                    Health = Health,
                    DistanceToTarget = DistanceToTarget,
                    BufferZone = BufferZone,
                    ResetTimer = ResetTime,
                    Status = ActionStatus.Idle,
                   UpdatePostition = true,
                   CanPatrol =true
                
                };
                
            }
            //NPC.AddComponent<BaseIAUSAuthoring>();
            return NPC;

        }

    }
}