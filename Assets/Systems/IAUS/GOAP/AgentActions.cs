using System;
using System.Collections.Generic;

namespace IAUS.Core.GOAP
{
    public class AgentActions
    {
        AgentActions(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public float cost { get; private set; }

        public HashSet<AgentBelief> Preconditions { get; } = new();
        public HashSet<AgentBelief> Effects { get; } = new();

        private IActionStrategy strategy;

        public bool Complete => strategy.Complete;

        public void Start() => strategy.Start();

        public void Update(float deltaTime)
        {
            if(strategy.CanPerform)
                strategy.Update(deltaTime);
            if(!strategy.Complete) return;
            foreach (var effect in Effects)
            {
                effect.Evalute();
            }
        }
        
        public void Stop() => strategy.Stop();


        public class Builder
        {
            private readonly AgentActions action;

            public Builder(string Name)
            {
                action = new AgentActions(Name)
                {
                    cost = 1
                };
            }

            public Builder WithCost(float cost)
            {
                action.cost = cost;
                return this;
            }

            public Builder WithStrategy(IActionStrategy strategy)
            {
                action.strategy = strategy;
                return this;
            }     
            public Builder AddPreconditions(AgentBelief precondition)
            {
                action.Preconditions.Add(precondition);
                return this;
            }     
            public Builder AddEffect(AgentBelief effect)
            {
                action.Effects.Add(effect);
                return this;
            }
        }

      
    }
}