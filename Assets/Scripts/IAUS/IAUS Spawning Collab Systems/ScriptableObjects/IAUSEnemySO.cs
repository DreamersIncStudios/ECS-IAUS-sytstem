using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpawnerSystem.ScriptableObjects;
using IAUS.ECS2;
using ProjectRebirth.Bestiary.Interfaces;
using UnityEngine.AI;
using Components.MovementSystem;
using IAUS.SpawnerSystem.interfaces;


namespace IAUS.SpawnerSystem
{

    public class IAUSEnemySO : Enemy, iMovement, iPatrol, iWait, iFollow
    {
        [SerializeField] ActiveAIStates NPCAIStates;
        [Header("Patrol")]
     
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
        [SerializeField] int maxInfluenceAtPoint;
        [Header("Wait")]
        [SerializeField] float timeToWait = 20;
        [SerializeField]
        ConsiderationData waitTimer = new ConsiderationData()
        {
            responseType = ResponseType.Logistic,
            M = 50,
            K = -1f,
            B = .91f,
            C = .2f
        };
        [Header("FollowCharacter")]

        [SerializeField] float distanceToMantainFromTarget;
        public float DistanceToMantainFromTarget { get { return distanceToMantainFromTarget; } }

        [Header("Movement")]
        [SerializeField] float speed = 4.5f;
        [SerializeField] float stoppingDistance = .5f;
        [SerializeField] float acceleration = 8f;
        [SerializeField] float height = 1f;
        [SerializeField] float radius = .5f;
       //  Movement _movement;
        [SerializeField] bool useNavMeshAgent = true;
        float resetTime = 5;// decide if this standardized or not
        public ConsiderationData Health { get { return health; } }

        public ConsiderationData DistanceToTarget { get { return distanceToPatrolTarget; } }

        public float BufferZone{ get { return bufferZone; } }
        public int MaxInfluenceAtPoint { get { return maxInfluenceAtPoint; } }
        public float ResetTime { get { return resetTime; } }

        public float MaxSpeed { get { return speed; }set { speed = value; } }
        public float StoppingDistance { get { return stoppingDistance; } set { stoppingDistance = value; } }
        public float Acceleration { get { return acceleration; } set { acceleration = value; } }
        public bool UseNavMeshAgent { get { return useNavMeshAgent; } set { useNavMeshAgent = value; } }

        public float Height { get { return height; }  set { height = value; } }
        public float Radius { get { return radius; } set { radius = value; } }
        public float TimeToWait { get { return timeToWait; } private set { timeToWait = value; } }

        public ConsiderationData WaitTimer { get { return waitTimer; } set { waitTimer = value; } }

        public GameObject SpawnAsLeader(Vector3 Position, List<PatrolBuffer> Points) {
            GameObject NPC = Spawn(Position);
                   IAUSAuthoring AIAuthor = NPC.AddComponent<IAUSAuthoring>();
            //replace with CharacterSystem
            AIAuthor.CurHealth = AIAuthor.MaxHealth = BaseHealth;
            AIAuthor.MaxMana = AIAuthor.CurMana = BaseMana;
   

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
                    CanPatrol = true,
                    LeaderUpdate = true
                   
                
                };
                AIAuthor.Waypoints = Points;
                AIAuthor.Move = new Movement() 
                {
                    MovementSpeed = speed,
                   StoppingDistance= stoppingDistance,
                   Acceleration=acceleration,
                    MaxInfluenceAtPoint = MaxInfluenceAtPoint
                };

                if (UseNavMeshAgent)
                {
                    NavMeshAgent agent = NPC.AddComponent<NavMeshAgent>();
                    agent.speed = MaxSpeed;
                    agent.height = Height;
                    agent.radius = Radius;
                }
            }

            if (NPCAIStates.Wait) 
            {
                AIAuthor.Wait = new WaitTime()
                {
                    Health = health,
                    WaitTimer = WaitTimer,
                    TimeToWait = TimeToWait,
                    Status = ActionStatus.Idle,
                    ResetTimer=resetTime

                };
            
            }
                //Uncommment once scene is built
            NPC.AddComponent<BaseIAUSAuthoring>();
            return NPC;

        }
        public GameObject SpawnAsSquadMember(Vector3 Position)
        {
            GameObject NPC = Spawn(Position);
             IAUSAuthoring AIAuthor = NPC.AddComponent<IAUSAuthoring>();
            NPCAIStates.Follow = true;
            NPCAIStates.Patrol = false;
            AIAuthor.StatesToAdd = NPCAIStates;

            AIAuthor.CurHealth = AIAuthor.MaxHealth = BaseHealth;
            AIAuthor.MaxMana = AIAuthor.CurMana = BaseMana;
            if (UseNavMeshAgent)
            {
                NavMeshAgent agent = NPC.AddComponent<NavMeshAgent>();
                agent.speed = MaxSpeed;
                agent.height = Height;
                agent.radius = Radius;
            }

            if (NPCAIStates.Follow) {
                AIAuthor.Follow = new FollowCharacter()
                {
                    Health= health,
                    DistanceToMantainFromTarget = 7.5f,
                    IsTargetMoving=false,
                    Status=ActionStatus.Idle,
                    ResetTimer = resetTime

                };
            }

            if (NPCAIStates.Wait)
            {
                AIAuthor.Wait = new WaitTime()
                {
                    Health = health,
                    WaitTimer = WaitTimer,
                    TimeToWait = TimeToWait,
                    Status = ActionStatus.Idle,
                    ResetTimer = resetTime

                };

            }
            NPC.AddComponent<BaseIAUSAuthoring>();

            return NPC;
    
        }


       
    }
}