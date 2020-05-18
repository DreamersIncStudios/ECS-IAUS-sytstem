using UnityEngine;
using SpawnerSystem.ScriptableObjects;
using SpawnerSystem;
using ProjectRebirth.Bestiary.Interfaces;
using IAUS.ECS2;

namespace ProjectRebirth.Bestiary
{

    
    public class EnemyNPC : NPCBase, AIDriven,iPatrol, iWait
    {
        [SerializeField] ConsiderationData _health = new ConsiderationData() { };
        [SerializeField] ConsiderationData _distanceToTarget;
        [SerializeField] ConsiderationData _waitTimer;
        [SerializeField] float _bufferZone;
        [SerializeField] float _timeToWait;
        [SerializeField] float _resetTimer;
        [SerializeField] ActiveAIStates _activeAIStates;

        public ActiveAIStates activeAIStates { get { return _activeAIStates; } }
        public ConsiderationData Health { get { return _health; } }

        public ConsiderationData DistanceToTarget { get {return _distanceToTarget; } }

        public float BufferZone { get { return _bufferZone; } }

        public float ResetTime { get { return _resetTimer; } }

        public float TimeToWait { get { return _timeToWait; } }

        public ConsiderationData WaitTimer { get { return _waitTimer; } }
    }
}