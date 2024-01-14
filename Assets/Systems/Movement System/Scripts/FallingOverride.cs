using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DreamersInc.MovementSys
{
    public class FallingOverride : MonoBehaviour
    {
        bool isPlayer => this.gameObject.CompareTag("Player");
        Rigidbody rb;
        NavMeshAgent agent;
        Animator animator;
        float fallingTime;
        // Start is called before the first frame update
        void Start()
        {
            rb= GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            if(!isPlayer )
            {
                agent = GetComponent<NavMeshAgent>();
            }
            fallingTime = 0.0f;
        }

        // Update is called once per frame
        void Update()
        {
            if (!animator.GetBool("OnGround"))
            {
                fallingTime += Time.deltaTime;
            }
            else {
                fallingTime = 0.0f;
            }

            if(fallingTime >= 3.50f)
            {
                Reset();

            }
        }
        private void Reset()
        {
            var position = GameObject.FindGameObjectWithTag("Respawn").transform.position; // Todo change to finding closer Respawn point. 
            rb.useGravity = false;
            rb.velocity = Vector3.zero; ;
            if (isPlayer) {
                transform.position = position;
            }
            else
            {
                agent.Warp(position);
            }
            fallingTime = 0.0f;
            rb.useGravity = true;
        }
    }
}
