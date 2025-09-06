using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using AISenses;
using AISenses.VisionSystems;
using Global.Component;
using Stats.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Combinators
{
    public readonly struct TargetCtx
    {
        public readonly float3 Origin;
        public readonly float Angle;
        public readonly float3 Direction;
        public readonly int FactionID;
        public readonly float r2;
        public readonly CollisionFilter Filter;
        public readonly CollisionWorld World;
        public TargetCtx( LocalTransform transform, Vision vision,int factionID, CollisionWorld world, CollisionFilter filter)
        {
            this.Origin = transform.Position;
            Direction = transform.Forward();
            this.Angle = vision.ViewAngle;
            this.World = world;
            this.r2 = vision.ViewRadius * vision.ViewRadius;
            this.Filter = filter;
            FactionID = factionID;
        }

    
    }
    interface IPred
    {
        bool Test(AIStat stat, LocalTransform transform, in TargetCtx ctx);
        List<TargetQuadrantData> Test(List<TargetQuadrantData> targets, LocalTransform transform, in TargetCtx ctx);
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
                public bool Test(AIStat stat, LocalTransform transform, in TargetCtx ctx)
                {
                    return a.Test(stat, transform, in ctx) && b.Test(stat, transform, in ctx);
                }
                
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public List<TargetQuadrantData> Test(List<TargetQuadrantData> targets, LocalTransform transform,
                    in TargetCtx ctx)
                {
                    var afterA = a.Test(targets, transform, in ctx);
                    return b.Test(afterA, transform, in ctx);
                }
            }
            // Fluent Builder
            readonly struct Chain<TPred> where TPred : struct, IPred
            {
                public readonly TPred pred;

                public Chain(TPred pred)
                {
                    this.pred = pred;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public Chain<And<TPred, Tnext>> And<Tnext>(Tnext n) where Tnext : struct, IPred =>
                    new(new And<TPred, Tnext>(pred, n));
                public TPred Build() => pred;
            }

            static class PredChain
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static Chain<TLeaf> Start<TLeaf>(TLeaf leaf) where TLeaf : struct, IPred =>
                    new(leaf);
            }
            // Predicates

            readonly struct InRange : IPred
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public readonly bool Test(AIStat stat,LocalTransform transform, in TargetCtx ctx)=>
                    math.lengthsq(transform.Position-ctx.Origin)<=ctx.r2;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public readonly List<TargetQuadrantData> Test(List<TargetQuadrantData> targets,
                    LocalTransform transform, in TargetCtx ctx)
                {
                    var outList = new List<TargetQuadrantData>();
                    foreach (var target in targets)
                    {
                        if (math.lengthsq(transform.Position-target.Position)<=ctx.r2)
                        {
                            outList.Add(target);
                        }
                    }
                    return outList;
                }
            }
            readonly struct IsAlive : IPred
            {
                public bool Test(AIStat stat, LocalTransform transform, in TargetCtx ctx)
                {
                    return stat.CurHealth>10;
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                
                public readonly List<TargetQuadrantData> Test(List<TargetQuadrantData> targets,
                    LocalTransform transform, in TargetCtx ctx)
                {
                    var outlist = new List<TargetQuadrantData>();
                    foreach (var target in targets)
                    {
                        if (target.TargetInfo.IsAlive)
                        {
                            outlist.Add(target);
                        }
                    }

                    return outlist;
                }
            }

            readonly struct InViewCone : IPred
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public readonly bool Test(AIStat stat, LocalTransform transform, in TargetCtx ctx)
                {
                    var dirToTarget = ((Vector3)AIStat.Position -(Vector3)(ctx.Origin+ new float3(0,1,0))).normalized;
                    return Vector3.Angle(ctx.Direction, dirToTarget) < ctx.Angle;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]

                public readonly List<TargetQuadrantData> Test(List<TargetQuadrantData> targets,
                    LocalTransform transform, in TargetCtx ctx)
                {
                    var outList = new List<TargetQuadrantData>();
                    foreach (var target in targets)
                    {
                        var dirToTarget = ((Vector3)target.Position -(Vector3)(ctx.Origin+ new float3(0,1,0))).normalized;
                        if (!(Vector3.Angle(ctx.Direction, dirToTarget) < ctx.Angle)) continue;
                        outList.Add(target);
                    }
                    return outList;
                }
            }

            readonly struct IsEnemy : IPred
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public readonly bool Test(AIStat stat, LocalTransform transform, in TargetCtx ctx)
                {
                return true;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]

                public readonly List<TargetQuadrantData> Test(List<TargetQuadrantData> targets,
                    LocalTransform transform, in TargetCtx ctx)
                {
                    var outList = new List<TargetQuadrantData>();
                    foreach (var target in targets)
                    {
                        var dirToTarget = ((Vector3)target.Position -(Vector3)(ctx.Origin+ new float3(0,1,0))).normalized;
                        if (!(Vector3.Angle(ctx.Direction, dirToTarget) < ctx.Angle)) continue;
                        outList.Add(target);
                    }
                    return outList;
                }
            }
            readonly struct IsFriendly : IPred
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public readonly bool Test(AIStat stat, LocalTransform transform, in TargetCtx ctx)
                {
                    return true;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]

                public readonly List<TargetQuadrantData> Test(List<TargetQuadrantData> targets,
                    LocalTransform transform, in TargetCtx ctx)
                {
                    var outList = new List<TargetQuadrantData>();
                    foreach (var target in targets)
                    {
                       
                    }
                    return outList;
                }
            }

            readonly struct InViewRayCast : IPred
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public readonly bool Test(AIStat stat, LocalTransform transform, in TargetCtx ctx)
                {
                    return true;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]

                public readonly List<TargetQuadrantData> Test(List<TargetQuadrantData> targets,
                    LocalTransform transform, in TargetCtx ctx)
                {
                    var outList = new List<TargetQuadrantData>();
                  
                    foreach (var target in targets)
                    {
                       var ray =CreateRaycastInput(transform, target.TargetInfo, ctx.Filter);
                       if (!ctx.World.CastRay(ray, out RaycastHit raycastHit)) continue;
                       if(raycastHit.Entity!=target.Entity) continue;
                       outList.Add(target);
                    }

                    return outList;
                }


               private RaycastInput CreateRaycastInput(LocalTransform transform, AITarget targetData,
                   CollisionFilter filter)
                {
                    return new RaycastInput()
                    {
                        Start = transform.Position + new float3(0, 1, 0) + transform.Forward() * 3f,
                        End = transform.Position + targetData.CenterOffset,
                        Filter = filter
                    };
                }
            }
}
