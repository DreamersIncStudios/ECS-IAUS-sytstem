using System;
using System.Collections.Generic;
using System.Linq;
using AISenses;
using AISenses.VisionSystems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.Core.GOAP
{
    public class AttackGoapAgent : GoapAgent
    {
        public AgentGoal lastGoal { get; set; }
        public AgentGoal CurrentGoal { get; set; }
        public ActionPlan ActionPlan { get; set; }
        public AgentAction currentAction { get; set; }
        public Dictionary<string, AgentBelief> Beliefs { get; set; }
        public HashSet<AgentAction> actions { get; set; }
        public HashSet<AgentGoal> goals { get; set; }
        private IGoapPlanner gPlanner;
        
        public bool CapableOfMelee, CapableOfMagic,CapableOfProjectile;
        public float3 TargetPosition, MyPosition;

        private bool InAttackRange(AttackType type)
        {
            var inRange = type switch
            {
                AttackType.Melee => 5 < Vector3.Distance(TargetPosition, MyPosition) && CapableOfMelee,
                AttackType.Range => 50 < Vector3.Distance(TargetPosition, MyPosition) && CapableOfProjectile,
                AttackType.Magic => 30 < Vector3.Distance(TargetPosition, MyPosition) && CapableOfMagic,
                _ => false
            };

            return inRange;
        }

        public void Update()
        {
            if (currentAction == null)
            {
                Debug.Log("Calculating any potential new plan ");
                CalculatePlan();
                if (ActionPlan != null && ActionPlan.Actions.Count > 0)
                {
                    CurrentGoal = ActionPlan.AgentGoal;
                    currentAction = ActionPlan.Actions.Pop();
                    currentAction.Start();
                    Debug.Log($"Goal: {CurrentGoal.Name} with {ActionPlan.Actions.Count} actions in plan");
                    Debug.Log($"Popped action: {currentAction.Name}");
                    // Verify all precondition effects are true
                    if (currentAction.Preconditions.All(b => b.Evaluate())) {
                        currentAction.Start();
                    } else {
                        Debug.Log("Preconditions not met, clearing current action and goal");
                        currentAction = null;
                        CurrentGoal = null;
                    }
                }
            }
        }

        private void CalculatePlan()
        {
            var priorityLevel = CurrentGoal?.Priority ?? 0;
            var goalsToCheck = goals;
            if (CurrentGoal != null)
            {
                goalsToCheck = new HashSet<AgentGoal>(goals.Where(g => g.Priority > priorityLevel));
            }

            var potentialPlan = gPlanner.Plan(this, goalsToCheck, lastGoal);
            if (potentialPlan != null)
            {
                ActionPlan = potentialPlan; 
            }
        }

        public void SetupBeliefs()
        {
            BeliefFactory factory = new BeliefFactory(this, Beliefs);
            
            factory.AddBelief("Nothing", () => false);
            factory.AddBelief("IsCapableOfMelee", () => CapableOfMelee);
            factory.AddBelief("IsCapableOfMagic", () => CapableOfMagic);
            factory.AddBelief("IsCapableOfProjectile", () => CapableOfProjectile);
            factory.AddBelief("InRangeForMelee", () => InAttackRange(AttackType.Melee));
            factory.AddBelief("InRangeForMagic", () => InAttackRange(AttackType.Magic));
            factory.AddBelief("InRangeForProjectile", () => InAttackRange(AttackType.Range));
        }

        public void SetupActions()
        {
            actions = new HashSet<AgentAction>();
            actions.Add(new AgentAction.Builder("Relax")
                .WithStrategy(new IdleStrategy(5))
                .AddEffect(Beliefs["Nothing"])
                .Build());
            actions.Add(new AgentAction.Builder("Move To Melee Range")
                .WithStrategy(new GotoLocation())
                .AddEffect(Beliefs["InRangeForMelee"])
                .Build());
            actions.Add(new AgentAction.Builder("Move To Magic Range")
                .WithStrategy(new GotoLocation())
                .AddEffect(Beliefs["InRangeForMagic"])
                .Build());
            actions.Add(new AgentAction.Builder("Move To Projectile Range")
                .WithStrategy(new GotoLocation())
                .AddEffect(Beliefs["InRangeForProjectile"])
                .Build());
        }

        public void SetupGoals()
        {
            goals = new HashSet<AgentGoal>();
            goals.Add(new AgentGoal.Builder("Cooldown")
                .WithPriority(1)
                .AddDesiredEffect(Beliefs["Nothing"])
                .Build());
            goals.Add(new AgentGoal.Builder("Move to Melee Range")
                .WithPriority(1)
                .AddDesiredEffect(Beliefs["InRangeForMelee"])
                .Build());
        }
    }

    public enum AttackType
    {
        Melee,Range,Magic
    }

    public readonly partial struct AttackGOAPAspect: IAspect
    {
        private readonly RefRO<LocalTransform> transform;
        private readonly VisionAspect vision;
        public float3 TargetPosition => vision.TargetPosition(TargetAlignmentType.Enemy);
        public float3 MyPosition => transform.ValueRO.Position;
        public bool HasAttackTarget => vision.TargetEnemyTargetInRange();
    }
}