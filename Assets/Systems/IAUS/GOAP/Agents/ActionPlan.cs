using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using UnityEngine;

namespace IAUS.Core.GOAP
{
    public interface IGoapPlanner
    {
        ActionPlan Plan(GoapAgent remove, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null);

    }

    public class GoapPlanner : IGoapPlanner
    {
        public ActionPlan Plan(GoapAgent remove, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null)
        {
            var orderedGoals = goals
                .Where(g => g.DesiredEffects.Any(b => !b.Evaluate()))
                .OrderByDescending(g => g == mostRecentGoal ? g.Priority - 0.01 : g.Priority)
                .ToList();
            foreach (var goal in orderedGoals)
            {
                Node goalNode = new Node(null, null, goal.DesiredEffects, 0);
                
                // If we can find a path to goal, return the plan 
                if (FindPath(goalNode, remove.actions))
                {
                    if(goalNode.isLeafDead) continue;

                    var actionStack = new Stack<AgentAction>();
                    while (goalNode.Leaves.Count>0){}
                    {
                        var cheapestLeaf = goalNode.Leaves.OrderBy(leaf => leaf.Cost).First();
                        goalNode = cheapestLeaf;
                        actionStack.Push(cheapestLeaf.Action);
                    }
                    return new ActionPlan(goal, actionStack, goalNode.Cost);
                }

            }
            Debug.LogWarning("No plans found");
            return null;
        }

        private bool FindPath(Node parent, HashSet<AgentAction> actions)
        {
            foreach (var action in actions)
            {
                var requireEffects = parent.RequiredEffects;
                //Remove effects that are true
                requireEffects.RemoveWhere(b => b.Evaluate());
                //If no required effects to fulfil
                if (requireEffects.Count == 0)
                {
                    return true;
                }

                if (action.Effects.Any(requireEffects.Contains))
                {
                    var newRequiredEffects = new HashSet<AgentBelief>(requireEffects);
                    newRequiredEffects.ExceptWith(action.Effects);
                    newRequiredEffects.UnionWith(action.Preconditions);
                    var newAvailableAction = new HashSet<AgentAction>(actions);
                 newAvailableAction.Remove(action);
                 var newNode = new Node(parent, action, newRequiredEffects, parent.Cost + action.cost);
                 if (FindPath(newNode, newAvailableAction))
                 {
                     parent.Leaves.Add(newNode);
                     newRequiredEffects.ExceptWith(newNode.Action.Preconditions);
                 }

                 if (newRequiredEffects.Count == 0)
                     return true;
                }
            }

            return false;
        }
    }

    public class Node
    {
        public Node Parent { get; }
        public AgentAction Action { get; }
        public float Cost { get; }
        public HashSet<AgentBelief> RequiredEffects { get; }
        public List<Node> Leaves { get; }

        public bool isLeafDead => Leaves.Count == 0 && Action == null;
        
        public Node(Node parent, AgentAction action, HashSet<AgentBelief> effects, float cost)
        {
            Parent = parent;
            Action = action;
            RequiredEffects = new HashSet<AgentBelief>(effects);
            Cost = cost;
            Leaves = new List<Node>();
        }
    }

    public class ActionPlan
    {
        public AgentGoal AgentGoal { get; }
        public Stack<AgentAction> Actions { get; }

        public float TotalCost { get; set; }

        public ActionPlan(AgentGoal goal, Stack<AgentAction> actions, float totalCost)
        {
            AgentGoal = goal;
            Actions = actions;
            TotalCost = totalCost;
        }
    }
}