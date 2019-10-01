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
        public bool UseJobs;
        public ThreatMatrixSystem.Crew Crew;
        public NavMeshAgent NavAgent { get { return this.GetComponent<NavMeshAgent>(); } }
        public Stats.PlayerCharacter PC { get { return this.GetComponent<Stats.PlayerCharacter>(); } }
        public bool ExitTimerLoop { get; set; }
        [Header("Travel")]
        public List<Transform> Waypoints;
        public Transform Target;
        public float TimeAtLoc;
        [SerializeField]public float Timer { get; set; }
        public bool Waiting;
        public bool Moving;

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
        public Vector3 DirFromAngle(float AngleInDeg, bool AngleIsGlobal)
        {
            if (!AngleIsGlobal)
                AngleInDeg += transform.eulerAngles.y;
            return new Vector3(Mathf.Sin(AngleInDeg * Mathf.Deg2Rad), 0, Mathf.Cos(AngleInDeg * Mathf.Deg2Rad));
        }

    }
}