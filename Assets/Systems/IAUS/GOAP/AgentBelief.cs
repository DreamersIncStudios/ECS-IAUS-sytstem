using System;
using System.Collections.Generic;
using AISenses;
using IAUS.ECS.Component.Aspects;
using Unity.Mathematics;
using UnityEngine;

namespace IAUS.Core.GOAP
{
    public class BeliefFactory
    {
        private readonly Dictionary<string, AgentBelief> beliefs;
        private IAUSBlackboard agent;
        public BeliefFactory(GoapAgent agent, Dictionary<string, AgentBelief> beliefs)
        {
            this.beliefs = beliefs;
        }

        public void AddBelief(string Key, Func<bool> condition)
        {
            beliefs.Add(Key, new AgentBelief.Builder(Key).
                WithCondition(condition)
                .Build());
        }

        public void AddSensorBelief(string Key, ISensor sensor, TargetAlignmentType alignmentType)
        {
            beliefs.Add(Key, new AgentBelief.Builder(Key)
                    .WithCondition(() => sensor.IsInRange(alignmentType))
                .WithLocation(() => sensor.TargetPosition(alignmentType))
                .Build());
        }

        bool inRangeOf(Vector3 pos, float range) => Vector3.Distance(agent.Transform.ValueRO.Position, pos) < range;
        public void AddBelief(string Key, float distance, float3 location)
        {
            beliefs.Add(Key, new AgentBelief.Builder(Key).
                WithCondition(()=>inRangeOf(location, distance))
                .WithLocation(()=>location)
                .Build());
        }
    }

    public class AgentBelief
    {
        public string Name { get; }
        private Func<bool> condition = ()=>false;
        private Func<float3> observedLocation = () => float3.zero;
        public float3 Location => observedLocation();


        AgentBelief(string name)
        {
            Name = name;
        }

        public bool Evalute()
        {
            return condition();
        }

        public class Builder
        {
            private readonly AgentBelief belief;

            public Builder(string name)
            {
                belief = new AgentBelief(name);
            }

            public Builder WithCondition(Func<bool> condition)
            {
                belief.condition = condition;
                return this;
            }

            public Builder WithLocation(Func<float3> location)
            {
                belief.observedLocation = location;
                return this;
            }

            public AgentBelief Build()
            {
                return belief;
                
            }
        }
    }
}