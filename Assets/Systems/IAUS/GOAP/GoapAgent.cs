using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace IAUS.Core.GOAP
{
    public interface GoapAgent: IComponentData
    {
        AgentGoal lastGoal { get; set; }
        AgentGoal CurrentGoal { get; set; }

        public ActionPlan ActionPlan { get; set; }
        public AgentAction currentAction{ get; set; }

        public Dictionary<string, AgentBelief> Beliefs{ get; set; }
        public HashSet<AgentAction> actions{ get; set; }
        public HashSet<AgentGoal> goals{ get; set; }

        void SetupBeliefs()
        {
        }

        void SetupActions()
        {
        }

        void SetupGoals() { }

     
        void HandleTargetChanged(object sender, SensorEventManagement.OnTargetChanged e)
        {
            Debug.Log("Target Change, Clear current action and goal ");
            currentAction = null;
            CurrentGoal = null;
        }
    }
}