using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using IAUS.ECS.Component;
using Unity.Entities;
using Sirenix.Utilities;
using Unity.Physics;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;


namespace Combinators.Attack_AI
{
    public readonly struct PositionCTX : IContext
    {
        public float3 Origin { get; }
        public float3 Forward { get; }
        public float EffectiveAttackRange { get; }
        public float3 TargetPosition { get; }
        public Entity TargetEntity { get; }
        public float3 TargetOffset { get; }
        public readonly CollisionWorld World;
        public readonly CollisionFilter Filter;

        public PositionCTX(float3 origin, float3 forward, float effectiveAttackRange, float3 targetPosition, float3 targetOffset,
            Entity targetEntity, CollisionWorld world, CollisionFilter filter)
        {
            Origin = origin;
            Forward = forward;
            EffectiveAttackRange = effectiveAttackRange;
            TargetPosition = targetPosition;
            TargetEntity = targetEntity;
            World = world;
            Filter = filter;
            TargetOffset = targetOffset;
        }
    }

    public readonly struct CoverData : IEquatable<CoverData>
    {
        public CoverData(float3 position, Cover coverInfo)
        {
            this.Position = position;
            CoverInfo = coverInfo;
        }

        public Cover CoverInfo { get; }
        public float3 Position { get; }

        public bool Equals(CoverData other)
        {
            return CoverInfo.Equals(other.CoverInfo) && Position.Equals(other.Position);
        }

        public override bool Equals(object obj)
        {
            return obj is CoverData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CoverInfo, Position);
        }
    }

    interface IPositionPredicate
    {
        List<CoverData> Test(List<CoverData> Covers, in PositionCTX ctx);
    }

    //Combinators for Predicates
    readonly struct And<A, B> : IPositionPredicate where A : IPositionPredicate where B : IPositionPredicate
    {
        public readonly A a;
        public readonly B b;

        public And(A a, B b)
        {
            this.a = a;
            this.b = b;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<CoverData> Test(List<CoverData> targets,
            in PositionCTX ctx)
        {
            var afterA = a.Test(targets, in ctx);
            return b.Test(afterA, in ctx);
        }
    }

    readonly struct Not<A> : IPositionPredicate where A : struct, IPositionPredicate
    {
        public readonly A a;

        public Not(A a)
        {
            this.a = a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<CoverData> Test(List<CoverData> targets, in PositionCTX ctx)
        {
            if (targets.IsNullOrEmpty()) return targets;
            var neg = a.Test(targets, in ctx);
            if (neg.IsNullOrEmpty()) return targets;


            var outList = new List<CoverData>(targets.Count);
            foreach (var t in targets)
            {
                if (!neg.Contains(t))
                {
                    outList.Add(t);
                }
            }

            return outList;
        }
    }


    // Fluent Builder
    readonly struct Chain<TPred> where TPred : struct, IPositionPredicate
    {
        public readonly TPred pred;

        public Chain(TPred pred)
        {
            this.pred = pred;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Chain<And<TPred, Tnext>> And<Tnext>(Tnext n) where Tnext : struct, IPositionPredicate =>
            new(new And<TPred, Tnext>(pred, n));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Chain<Not<TPred>> Not() => new(new Not<TPred>(pred));

        public TPred Build() => pred;
    }

    static class PredChain
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Chain<TLeaf> Start<TLeaf>(TLeaf leaf) where TLeaf : struct, IPositionPredicate =>
            new(leaf);
    }
    // Predicates

    readonly struct InEffectiveAttackRange : IPositionPredicate
    {
        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly List<CoverData> Test(List<CoverData> targets, in PositionCTX ctx)
        {
            var outList = new List<CoverData>();
            foreach (var target in targets)
            {
                var dist = Vector3.Distance(target.Position, ctx.Origin);
                if (dist > ctx.EffectiveAttackRange) continue;
                outList.Add(target);
            }

            return outList;
        }
    }

    readonly struct CanSeeTargetStage : IPositionPredicate
    {
        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly List<CoverData> Test(List<CoverData> targets, in PositionCTX ctx)
        {
            var outList = new List<CoverData>();
            foreach (var target in targets)
            {
                foreach (var position in target.CoverInfo.CoverPoints)
                {
                    Debug.DrawLine(position+ new float3(0, 1, 0), ctx.TargetPosition + new float3(0, 1, 0), Color.red, 3f);
                    var ray = CreateRaycastInput(position, ctx.Forward, ctx.TargetPosition,  ctx.TargetOffset, 
                        ctx.Filter);
                    if (!ctx.World.CastRay(ray, out RaycastHit raycastHit)) continue;

                    if (!raycastHit.Entity.Equals(ctx.TargetEntity))
                        continue;
                    if (outList.Contains(target)) continue;
                    outList.Add(target);
                    break;
                }
            }

            return outList;
        }

        private RaycastInput CreateRaycastInput(float3 transform, float3 Forward, float3 targetPosition, float3 offset,
            CollisionFilter filter)
        {
            return new RaycastInput()
            {
                Start = transform + new float3(0, 1, 0) + Forward,
                End = targetPosition +offset,
                Filter = filter
            };
        }
    }
}