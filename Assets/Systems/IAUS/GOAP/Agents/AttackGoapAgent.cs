using System.Collections.Generic;
using AISenses;
using AISenses.VisionSystems;
using Unity.Entities;
using Unity.Transforms;

namespace IAUS.Core.GOAP
{
    public class AttackGoapAgent : GoapAgent
    {
        public AgentGoal lastGoal { get; set; }
        public AgentGoal CurrentGoal { get; set; }
        public ActionPlan ActionPlan { get; set; }
        public AgentActions CurrentAction { get; set; }
        public Dictionary<string, AgentBelief> Beliefs { get; set; }
        public HashSet<AgentActions> actions { get; set; }
        public HashSet<AgentGoal> goals { get; set; }

        
    }
    
    public readonly partial struct GOAPAspect: IAspect
    {
        private readonly RefRO<LocalTransform> transform;
        private readonly VisionAspect vision;
        
        
    }
}