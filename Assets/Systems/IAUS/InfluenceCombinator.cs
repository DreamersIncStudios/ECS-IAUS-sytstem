using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AISenses;
using DreamersIncStudio.FactionSystem;
using Unity.Entities;
using Unity.Transforms;

namespace Combinators.InfluenceCombinators
{
    public readonly struct InfoCtx
    {
        public readonly FactionNames FactionName;
        public readonly IEnumerable<Relationship> Relationships;
        public InfoCtx(FactionNames factionName,     DynamicBuffer<Factions> factionsBuffer)
        {
            FactionName = factionName;
            Relationships = new List<Relationship>();
            foreach (var factions in factionsBuffer)
            {
                if (factions.Faction != FactionName) continue;
                Relationships = factions.Relationships;
                break;
            }
        }
    }


    interface IPred
    {
        public bool Test(FactionNames factionName, in InfoCtx ctx);
        
    }
    
    //Combinators for Predicates
    readonly struct And<A,B>: IPred where A:IPred where B:IPred
    {
        public readonly A a;
        public readonly B b;
        public And(A a, B b)
        {
            this.a = a;
            this.b = b;
        }
        [method:MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Test(FactionNames faction, in InfoCtx ctx)
        {
            return a.Test(faction, in ctx) && b.Test(faction, in ctx);
        }
                
       
    }
    
    // Fluent Builder
    
    
    // Predicates
    readonly struct IsFriendly : IPred
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Test(FactionNames faction, in InfoCtx ctx)
        {
            foreach (var relationship in ctx.Relationships)
            {
                if (relationship.Faction != faction) continue;
                var affinity = relationship.Affinity switch
                {
                    < -75 => Affinity.Hate,
                    > -75 and < -35 => Affinity.Negative,
                    > -35 and < 35 => Affinity.Neutral,
                    > 35 and < 74 => Affinity.Positive,
                    > 75 => Affinity.Love,
                    _ => Affinity.Neutral
                };
                if (affinity is Affinity.Positive or Affinity.Love) return true;
            }
            return false;
        }
    }
}