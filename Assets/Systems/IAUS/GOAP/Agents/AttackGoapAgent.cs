using System;
using System.Collections.Generic;
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
                .WithStrategy()
                .AddEffect(Beliefs["Nothing"])
                .Build());
            actions.Add(new AgentAction.Builder("Move To Magic Range")
                .WithStrategy()
                .AddEffect(Beliefs["Nothing"])
                .Build());
            actions.Add(new AgentAction.Builder("Move To Projectile Range")
                .WithStrategy()
                .AddEffect(Beliefs["Nothing"])
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