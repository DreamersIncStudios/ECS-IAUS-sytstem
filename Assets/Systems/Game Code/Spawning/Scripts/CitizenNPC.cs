using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;
using AISenses.Authoring;
using Global.Component;
using AISenses;
using IAUS.ECS;
using Components.MovementSystem;
using IAUS.ECS.Component;
using DreamersInc.InflunceMapSystem;
using UnityEngine.AI;
using Unity.Entities;
using System.Threading.Tasks;
using System;
using Unity.Transforms;
using Utilities;
using Unity.Mathematics;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System.Linq;
using Unity.Physics;
using Unity.Physics.Authoring;

namespace IAUS.NPCScriptableObj
{
    [System.Serializable]
    //TODO Write Custom Property Drawers
    public class CitizenNPC
    {
        [Range(1, 500)]
        public int Count;
        public List<GameObject> Models;
        public AITarget Self;
        public List<AIStates> AIStatesAvailable;
        public MovementBuilderData GetMovement;
        public WaitBuilderData GetWait;
        public Movement AIMove;
        public Vision GetVision;
        public InfluenceComponent GetInfluence;
        public Perceptibility GetPerceptibility;
        public PhysicsInfo GetPhysicsInfo { get {
                return new PhysicsInfo()
                {
                    BelongsTo = belongsTo,
                    CollidesWith = collideWith
                };
                    } }
        public PhysicsCategoryTags belongsTo;
        public PhysicsCategoryTags collideWith;

        EntityArchetype citizen;
        bool loaded;
        public void LoadModals()
        {
            GameObject[] goLoaded = Resources.LoadAll("Players", typeof(GameObject)).Cast<GameObject>().ToArray();
            foreach (var go in goLoaded)
            {
                Models.Add(go);
            }
            loaded = true;
        }



    }
   
}