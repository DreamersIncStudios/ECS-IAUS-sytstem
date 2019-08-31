using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace IAUS
{
    public class CharacterContext : MonoBehaviour,IContext
    {

        public NavMeshAgent NavAgent { get { return this.GetComponent<NavMeshAgent>(); } }
        public List<Transform> Waypoints;
        public float TimeAtLoc;
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