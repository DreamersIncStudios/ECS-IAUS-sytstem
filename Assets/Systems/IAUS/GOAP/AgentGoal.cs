using System.Collections.Generic;

namespace IAUS.Core.GOAP
{
    public class AgentGoal
    {
        public string Name { get; }
        public float Priority { get; private set; }
        public HashSet<AgentBelief> DesiredEffects { get; } = new();

        AgentGoal(string name)
        {
            Name = name;
        }


        public class Builder
        {
            private readonly AgentGoal goal;

            public Builder(string name)
            {
                goal = new AgentGoal(name)
                {
                    Priority = 1
                };
            }

            public Builder WithPriority(float priority)
            {
                goal.Priority = priority;
                return this;
            }

            public Builder AddDesiredEffect(AgentBelief effect)
            {
                goal.DesiredEffects.Add(effect);
                return this;
            }

            public AgentGoal Build()
            {
                return goal;
            }
        }
    }
}