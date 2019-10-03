using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS.Jobs;
using IAUS.ECS.ComponentStates;
using UnityEngine.AI;
using Unity.Collections;

namespace IAUS.ECS.Utility.System
{
    [UpdateAfter(typeof(ScoreJob))]
    public class GruntUtility : ComponentSystem
    {
        public EntityQueryDesc Grunts = new EntityQueryDesc() {
            All = new ComponentType[] { typeof(MoveToComponent), typeof(WaitForComponent), typeof(NavMeshAgent) }
        };
        public EntityQuery g;

        protected override void OnUpdate()
        {

            Entities.With(GetEntityQuery(Grunts)).ForEach((Entity ent, NavMeshAgent agent, ref MoveToComponent move, ref WaitForComponent wait) => {
                var buffer = EntityManager.GetBuffer<Patrol>(ent);
                if (move.Target != agent.destination) { agent.SetDestination(move.Target); }
            });


        }
    }
}