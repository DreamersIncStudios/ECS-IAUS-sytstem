using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpawnerSystem.ScriptableObjects;
using IAUS.ECS2;
using ProjectRebirth.Bestiary.Interfaces;
using UnityEngine.AI;
using Components.MovementSystem;
using IAUS.SpawnerSystem.interfaces;
using Stats;
using Unity.Mathematics;


namespace IAUS.SpawnerSystem
{

    public class IAUSEnemySO : Enemy, iMovement, iPatrol, iWait, iFollow, IHealSelf
    {
        [SerializeField] ActiveAIStates NPCAIStates;
        [Header("Patrol")]

        [SerializeField]
        private ConsiderationData fullhealth = new ConsiderationData
        {
            responseType = ResponseType.Logistic,
            M = 50,
            K = -1,
            B = .91f,
            C = .35f
        };
        [SerializeField]
        private ConsiderationData healthTilDeath = new ConsiderationData
        {
            responseType = ResponseType.Logistic,
            M = 50,
            K = 1,
            B = .0f,
            C = .65f
        };
        [SerializeField]
        private
        ConsiderationData distanceToPatrolTarget = new ConsiderationData()
        {
            responseType = ResponseType.Logistic,
            M = 50,
            K = -.95f,
            B = .935f,
            C = .35f
        };
        [SerializeField] private float bufferZone = .5f;
        [SerializeField] private int maxInfluenceAtPoint;
        [Header("Wait")]
        [SerializeField] private float timeToWait = 20;
        [SerializeField]
        private ConsiderationData waitTimer = new ConsiderationData()
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
        [SerializeField] private float speed = 4.5f;
        [SerializeField] private float stoppingDistance = .5f;
        [SerializeField] private float acceleration = 8f;
        [SerializeField] private float height = 1f;
        [SerializeField] private float radius = .5f;
        //  Movement _movement;
        [SerializeField] private bool useNavMeshAgent = true;
        [SerializeField] private float3 _offset = new float3(0, .5f, 0);
        [Header("Heal Self")]
        [SerializeField] private float ItemUsageIntervals;
        [SerializeField] private int MaxCountOfItems;
        [SerializeField] private ConsiderationData HealIntervals = new ConsiderationData()
        {
            responseType = ResponseType.Logistic,
            M = 50,
            K = -1f,
            B = .91f,
            C = .2f
        };
        [SerializeField]
        private ConsiderationData InventoryCheck = new ConsiderationData()
        {
            responseType = ResponseType.Logistic,
            M = 50,
            K = 1,
            B = .0f,
            C = .65f
        };
        float resetTime = 5;// decide if this standardized or not
        public ConsiderationData FullHealth { get { return fullhealth; } }

        public ConsiderationData DistanceToTarget { get { return distanceToPatrolTarget; } }

        public float BufferZone{ get { return bufferZone; } }
        public int MaxInfluenceAtPoint { get { return maxInfluenceAtPoint; } }
        public float ResetTime { get { return resetTime; } }

        public float MaxSpeed { get { return speed; }set { speed = value; } }
        public float StoppingDistance { get { return stoppingDistance; } set { stoppingDistance = value; } }
        public float Acceleration { get { return acceleration; } set { acceleration = value; } }
        public bool UseNavMeshAgent { get { return useNavMeshAgent; } set { useNavMeshAgent = value; } }
        public float3 Offset { get { return _offset; } }
        public float Height { get { return height; }  set { height = value; } }
        public float Radius { get { return radius; } set { radius = value; } }
        public float TimeToWait { get { return timeToWait; } private set { timeToWait = value; } }

        public ConsiderationData WaitTimer { get { return waitTimer; } set { waitTimer = value; } }

        public ConsiderationData TimeBetweenHeals { get { return HealIntervals; } }

        public ConsiderationData RecoveryItemsInInventory { get { return InventoryCheck; } }

        public ConsiderationData HealthTilDeath { get { return healthTilDeath; } }
        public int FullInventoryofItem { get { return MaxCountOfItems; } }

        public float timeBetweenHeals { get { return ItemUsageIntervals; } }

        public GameObject SpawnAsLeader(Vector3 Position, List<PatrolBuffer> Points) {
            GameObject NPC = Spawn(Position);
                   IAUSAuthoring AIAuthor = NPC.AddComponent<IAUSAuthoring>();


            //replace with CharacterSystem


            AIAuthor.StatesToAdd = NPCAIStates;
            if (NPCAIStates.Patrol)
            {
                AIAuthor.PatrolState = new Patrol()
                {
                    Health = FullHealth,
                    DistanceToTarget = DistanceToTarget,
                    BufferZone = BufferZone,
                    ResetTimer = ResetTime,
                    Status = ActionStatus.Idle,
              //      UpdatePostition = true,
                    CanPatrol = true,
            //        LeaderUpdate = true
                   
                
                };
                AIAuthor.Waypoints = Points;
                AIAuthor.Move = new Movement()
                {
                    MovementSpeed = speed,
                    StoppingDistance = stoppingDistance,
                    Acceleration = acceleration,
                    MaxInfluenceAtPoint = MaxInfluenceAtPoint,
                    Offset = Offset
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
                    Health = fullhealth,
                    WaitTimer = WaitTimer,
                    TimeToWait = TimeToWait,
                    Status = ActionStatus.Idle,
                    ResetTimer=resetTime

                };
            
            }
            if (NPCAIStates.HealSelfViaItem) {
                AIAuthor.HealSelf = new HealSelfViaItem()
                {
                    Health = HealthTilDeath,
                    RecoveryItemsInInventory = RecoveryItemsInInventory,
                    TimeBetweenHeals = TimeBetweenHeals,
                    TimeBetweenHealsTimer= ItemUsageIntervals,
                    FullInventoryofItem=MaxCountOfItems,
                    ResetTimer = resetTime,
                    Status=ActionStatus.Idle
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
                    Health= fullhealth,
                    DistanceToMantainFromTarget = 7.5f,
                    IsTargetMoving = false,
                    Status=ActionStatus.Idle,
                    ResetTimer = resetTime

                };
            }

            if (NPCAIStates.Wait)
            {
                AIAuthor.Wait = new WaitTime()
                {
                    Health = fullhealth,
                    WaitTimer = WaitTimer,
                    TimeToWait = TimeToWait,
                    Status = ActionStatus.Idle,
                    ResetTimer = resetTime

                };

            }
            if (NPCAIStates.HealSelfViaItem)
            {
                AIAuthor.HealSelf = new HealSelfViaItem()
                {
                    Health = HealthTilDeath,
                    RecoveryItemsInInventory = RecoveryItemsInInventory,
                    TimeBetweenHeals = TimeBetweenHeals,
                    TimeBetweenHealsTimer = ItemUsageIntervals,
                    FullInventoryofItem = MaxCountOfItems,
                    ResetTimer = resetTime,
                       Status = ActionStatus.Idle
                };
            }

                NPC.AddComponent<BaseIAUSAuthoring>();

            return NPC;
    
        }


       
    }
}