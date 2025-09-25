using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AISenses;
using AISenses.VisionSystems;
using Sirenix.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Combinators.Interactable
{
    public readonly struct InteractableCTX : IContext
    {
        public float3 Origin { get; }
        public float3 Forward { get; }

        public InteractableCTX(float3 origin, float3 forward)
        {
            Origin = origin;
            Forward = forward;
        }
    }

    interface IInteractablePredicate
    {
        List<Target> Test(List<Target> targets, in InteractableCTX ctx);
    }

    //Combinators for Predicates
    readonly struct And<A, B> : IInteractablePredicate where A : IInteractablePredicate where B : IInteractablePredicate
    {
        public readonly A a;
        public readonly B b;

        public And(A a, B b)
        {
            this.a = a;
            this.b = b;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Target> Test(List<Target> targets,
            in InteractableCTX ctx)
        {
            var afterA = a.Test(targets, in ctx);
            return b.Test(afterA, in ctx);
        }
    }

    readonly struct Not<A> : IInteractablePredicate where A : struct, IInteractablePredicate
    {
        public readonly A a;

        public Not(A a)
        {
            this.a = a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Target> Test(List<Target> targets, in InteractableCTX ctx)
        {
            if (targets.IsNullOrEmpty()) return targets;
            var neg = a.Test(targets, in ctx);
            // Don't mutate 'targets' while enumerating 'neg' if they can reference the same list.
            // Build a new list containing items from 'targets' that are NOT in 'neg'.
            if (neg.IsNullOrEmpty()) return targets;

            // Use HashSet for O(1) membership checks
            var toExclude = new HashSet<Entity>();
            for (int i = 0; i < neg.Count; i++)
            {
                toExclude.Add(neg[i].Entity);
            }

            var outList = new List<Target>(targets.Count);
            foreach (var t in targets)
            {
                if (!toExclude.Contains(t.Entity))
                {
                    outList.Add(t);
                }
            }

            return outList;
        }
    }

    // Fluent Builder
    readonly struct Chain<TPred> where TPred : struct, IInteractablePredicate
    {
        public readonly TPred pred;

        public Chain(TPred pred)
        {
            this.pred = pred;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Chain<And<TPred, Tnext>> And<Tnext>(Tnext n) where Tnext : struct, IInteractablePredicate =>
            new(new And<TPred, Tnext>(pred, n));

        public Chain<Not<TPred>> Not() => new(new Not<TPred>(pred));

        public TPred Build() => pred;
    }

    static class PredChain
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Chain<TLeaf> Start<TLeaf>(TLeaf leaf) where TLeaf : struct, IInteractablePredicate =>
            new(leaf);
    }

    readonly struct FilterDuplicates : IInteractablePredicate
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Target> Test(List<Target> targets, in InteractableCTX ctx)
        {
            var outList = new List<Target>();
            foreach (var target in targets)
            {
                if (outList.Contains(target)) continue;
                outList.Add(target);
            }

            return outList;
        }
    }

    readonly struct SortByRange : IInteractablePredicate
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Target> Test(List<Target> targets, in InteractableCTX ctx)
        {
            var outList = targets;
            outList.Sort(new SortTargetsByDistanceTo());

            return outList;
        }
    }

    readonly struct SortByInfluence : IInteractablePredicate
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Target> Test(List<Target> targets, in InteractableCTX ctx)
        {
            var outList = targets;


            return outList;
        }
    }

    readonly struct IsAttackable : IInteractablePredicate
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Target> Test(List<Target> targets, in InteractableCTX ctx)
        {
            var outList = new List<Target>();
            foreach (var target in targets)
                if (target.TargetInfo.Attackable)
                    outList.Add(target);
            return outList;
        }
    }
}