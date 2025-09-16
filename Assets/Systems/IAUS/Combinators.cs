using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using AISenses;
using AISenses.VisionSystems;
using DreamersIncStudio.FactionSystem;
using Global.Component;
using Stats.Entities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Combinators
{
    public interface IContext
    {
        public float3 Origin { get; }
        public  float ViewAngle{ get; }
        public float3 Forward {get; }
        public float r2 {get; }
    }

    public readonly struct TargetCtx: IContext
    {
        public  float3 Origin { get;  }
        public  float ViewAngle{ get;  }
        public  float3 Forward{ get;  }
        public float r2{ get;  }
        public readonly CollisionFilter Filter;
        public readonly CollisionWorld World;
        public readonly List<Relationship> Relationships;
        public TargetCtx(LocalTransform transform, Vision vision, FactionNames factionID, CollisionWorld world,
            DynamicBuffer<Factions> factionsBuffer, CollisionFilter filter)
        {
            this.Origin = transform.Position;
            Forward = transform.Forward();
            this.ViewAngle = vision.ViewAngle;
            this.World = world;
            this.r2 = vision.ViewRadius * vision.ViewRadius;
            this.Filter = filter;
            Relationships = new List<Relationship>();
            foreach (var factions in factionsBuffer)
            {
                if (factions.Faction != factionID) continue;
                foreach (var relationship in factions.Relationships)
                {
                    Relationships.Add(relationship);
                }

                break;
            }
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

            readonly struct IsNotSelf : IPred
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public readonly bool Test(AIStat stat, LocalTransform transform, in TargetCtx ctx)
                {
                    return !ctx.Origin.Equals(transform.Position);
                }
                
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public readonly List<TargetQuadrantData> Test(List<TargetQuadrantData> targets,
                    LocalTransform transform, in TargetCtx ctx)
                {
                    var outList = new List<TargetQuadrantData>();
                    foreach (var target in targets)
                    {
                        if (ctx.Origin.Equals(target.Position)) continue;
                        outList.Add(target);
                    }
                    return outList;
                }
            }

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
                    for (var index = 0; index < targets.Count; index++)
                    {
                        var target = targets[index];
                        if (ctx.Origin.Equals(target.Position)) continue;
                        if (math.lengthsq(transform.Position - target.Position) > ctx.r2)
                            continue;
                        target.Distance = math.length(transform.Position - target.Position);
                         outList.Add(target);
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
                       //todo add health check
                            outlist.Add(target);
                        
                    }

                    return outlist;
                }
            }

            readonly struct InViewCone : IPred
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public readonly bool Test(AIStat stat, LocalTransform transform, in TargetCtx ctx)
                {
                    var dirToTarget = ((Vector3)transform.Position -(Vector3)(ctx.Origin+ new float3(0,1,0))).normalized;
                    return Vector3.Angle(ctx.Forward, dirToTarget) < ctx.ViewAngle;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]

                public readonly List<TargetQuadrantData> Test(List<TargetQuadrantData> targets,
                    LocalTransform transform, in TargetCtx ctx)
                {
                    var outList = new List<TargetQuadrantData>();
                    foreach (var target in targets)
                    {
                        var dirToTarget = ((Vector3)target.Position -(Vector3)(ctx.Origin+ new float3(0,1,0))).normalized;
                        if (!(Vector3.Angle(ctx.Forward, dirToTarget) < ctx.ViewAngle)) continue;
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
                        foreach (var relationship in ctx.Relationships)
                        {
                            if (relationship.Faction != target.TargetInfo.FactionID) continue;
                            var affinity= relationship.Affinity switch
                            {
                                < -75 => Affinity.Hate,
                                > -75 and < -35 => Affinity.Negative,
                                > -35 and < 35 => Affinity.Neutral,
                                > 35 and < 74 => Affinity.Positive,
                                > 75 => Affinity.Love,
                                _ => Affinity.Neutral
                            };
                            if(affinity is Affinity.Positive or Affinity.Love) continue;
                            outList.Add(target);
                        }
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
                        foreach (var relationship in ctx.Relationships)
                        {
                            if (relationship.Faction != target.TargetInfo.FactionID) continue;
                            var affinity= relationship.Affinity switch
                            {
                                < -75 => Affinity.Hate,
                                > -75 and < -35 => Affinity.Negative,
                                > -35 and < 35 => Affinity.Neutral,
                                > 35 and < 74 => Affinity.Positive,
                                > 75 => Affinity.Love,
                                _ => Affinity.Neutral
                            };
                            if(affinity is Affinity.Negative or Affinity.Hate) continue;
                            outList.Add(target);
                        }
                    }
                    return outList;
                }
            }

            readonly struct SetAffinty : IPred
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                
                public bool Test(AIStat stat, LocalTransform transform, in TargetCtx ctx)
                {
                    throw new System.NotImplementedException();
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]

                public List<TargetQuadrantData> Test(List<TargetQuadrantData> targets, LocalTransform transform, in TargetCtx ctx)
                {
                    throw new System.NotImplementedException();
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
                       var ray =CreateRaycastInput(ctx.Origin, ctx.Forward,target, ctx.Filter);
                       if (!ctx.World.CastRay(ray, out RaycastHit raycastHit)) continue;
                       if(!raycastHit.Entity.Equals(target.Entity)) continue;
                       outList.Add(target);
                    }

                    return outList;
                }


               private RaycastInput CreateRaycastInput(float3 transform, float3 Forward, TargetQuadrantData targetData,
                   CollisionFilter filter)
                {
                    return new RaycastInput()
                    {
                        Start = transform + new float3(0, 1, 0) + Forward * 3f,
                        End =  targetData.Position + targetData.TargetInfo.CenterOffset ,
                        Filter = filter
                    };
                }
            }
}
