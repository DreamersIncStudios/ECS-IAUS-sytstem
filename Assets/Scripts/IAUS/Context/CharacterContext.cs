using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace IAUS
{
    public class CharacterContext : MonoBehaviour, IContext
    {
        #region Variables
        //rewrite code to passs in consideratiors

        public ThreatMatrixSystem.Crew Crew;
        public NavMeshAgent NavAgent { get { return this.GetComponent<NavMeshAgent>(); } }
        [Header("Travel")]
        public List<Transform> Waypoints;
        public Transform PointOfInterest;
        public float TimeAtLoc;
        [SerializeField]public float Timer { get; set; }


        [Header("Detection")]
        public float viewRadius;
        [Range(0, 360)]
        public float viewAngle;
        public float EngageRadius;
        [Range(0, 360)]
        public float EngageViewAngle; //TBA
        public LayerMask TargetMask;
        public LayerMask ObstacleMask;

        public List<Transform> VisibleTargets;
        [Range(0.0f, 2.0f)]
        public float DetectionLevel = 0.0f;
        float IntialDL;
        public Vector3 SearchLocation;
        public bool LookHere;
        #endregion

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}