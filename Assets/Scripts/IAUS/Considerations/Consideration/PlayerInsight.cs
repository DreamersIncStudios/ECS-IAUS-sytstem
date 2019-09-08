using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IAUS.Considerations
{
    /* Current Design Looks for all the enemies in the area and determines if they can be seen.
     Score is by the distance to the Agent
     This will be rewritten to account for the Threat rating at a later date
     */
    public class PlayerInSight : ConsiderationBase
    {

        public float viewRadius { get {return Agent.viewRadius; } }
        public float viewAngle { get { return Agent.viewAngle; } }
        public float EngageRadius { get { return Agent.EngageRadius; } }
        public float EngageViewAngle { get { return Agent.EngageViewAngle; } }
        public LayerMask TargetMask { get { return Agent.TargetMask; } }
        public LayerMask ObstacleMask { get {return Agent.ObstacleMask; } }
        public Transform AgentPos { get { return Agent.gameObject.transform; } }

public List<Transform> VisibleTargets { get; }
        public float DetectionLevel { get { return Agent.DetectionLevel; } }


        public override void Consider()
        {
            // Add player Detection algorithm
            // For testing set to .5
            Score = .5f;
        }

        //
        List<float> PossibleScores;
        void LookForEnemies() {
            VisibleTargets.Clear();

            Collider[] TargetInViewRadius = Physics.OverlapSphere(Agent.gameObject.transform.position, viewRadius,TargetMask);
            foreach (Collider col in TargetInViewRadius) {
                Vector3 dirToTarget = (col.transform.position = AgentPos.position).normalized;
                if (Vector3.Angle(AgentPos.forward, dirToTarget) < viewAngle / 2.0f) {
                    float dist = Vector3.Distance(AgentPos.position, col.transform.position);
                    if (!Physics.Raycast(AgentPos.position, dirToTarget, dist, ObstacleMask)) {
                        VisibleTargets.Add(col.transform); 
                    }
                }
            }
        }

        public override void Output(float input)
        {
        }
    }
}
