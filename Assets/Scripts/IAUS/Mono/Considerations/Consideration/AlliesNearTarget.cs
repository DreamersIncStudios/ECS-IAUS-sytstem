using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAUS.Considerations
{
    //This need to run after player insight
    public class AlliesNearTarget : ConsiderationBase
    {
        public int Range;
        public int MaxAllyies;
        List<Transform> VisibleTargets { get { return Agent.VisibleTargets; } }
        LayerMask IgnoreLayer { get { return Agent.ObstacleMask; } }
        public override void Consider()
        {
            if (VisibleTargets.Count>0) {
                foreach (Transform target in VisibleTargets) {
                    Collider[] GOnearTarget = Physics.OverlapSphere(target.position, Range);
                    List<CharacterContext> characterContexts = new List<CharacterContext>();
                    foreach (Collider GO in GOnearTarget) {
                        if (GO.GetComponent<CharacterContext>() != null) {
                            characterContexts.Add(GO.GetComponent<CharacterContext>());
                        }
                    }

                }

            }
            else { Score = 0.0f; }
        }

    }
}