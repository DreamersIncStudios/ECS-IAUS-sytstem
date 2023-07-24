using AISenses.VisionSystems.Combat;
using Components.MovementSystem;
using IAUS.ECS.Component;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Utilities.ReactiveSystem;

namespace IAUS.ECS.Systems.Reactive
{
    public partial class AttackSystem : SystemBase
    {
        EntityQuery meleeTagAdded;
        EntityQuery magicTagAdded;
        EntityQuery magicMeleeTagAdded;
        EntityQuery rangeTagAdded;

        protected override void OnCreate()
        {
            base.OnCreate();
            meleeTagAdded = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackState)), ComponentType.ReadWrite(typeof(AttackActionTag)), ComponentType.ReadWrite(typeof(Movement))
                , ComponentType.ReadOnly(typeof(LocalTransform)), ComponentType.ReadOnly(typeof(meleeAttackTag))
                },
                None = new ComponentType[] { ComponentType.ReadWrite(typeof(AIReactiveSystemBase<meleeAttackTag, AttackState, MeleeTagReactor>.StateComponent)) }
            });

            magicTagAdded = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackState)), ComponentType.ReadWrite(typeof(AttackActionTag)), ComponentType.ReadWrite(typeof(Movement))
                , ComponentType.ReadOnly(typeof(LocalTransform)), ComponentType.ReadOnly(typeof(magicAttackTag))
                },
                None = new ComponentType[] { ComponentType.ReadWrite(typeof(AIReactiveSystemBase<magicAttackTag, AttackState, MagicTagReactor>.StateComponent)) }
            });

            magicMeleeTagAdded = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackState)), ComponentType.ReadWrite(typeof(AttackActionTag)), ComponentType.ReadWrite(typeof(Movement))
                , ComponentType.ReadOnly(typeof(LocalTransform)), ComponentType.ReadOnly(typeof(magicmeleeAttackTag))
                },
                None = new ComponentType[] { ComponentType.ReadWrite(typeof(AIReactiveSystemBase<magicmeleeAttackTag, AttackState, MagicMeleeTagReactor>.StateComponent)) }
            });

            rangeTagAdded = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackState)), ComponentType.ReadWrite(typeof(AttackActionTag)), ComponentType.ReadWrite(typeof(Movement))
                , ComponentType.ReadOnly(typeof(LocalTransform)), ComponentType.ReadOnly(typeof(rangeAttackTag))
                },
                None = new ComponentType[] { ComponentType.ReadWrite(typeof(AIReactiveSystemBase<rangeAttackTag, AttackState, RangeTagReactor>.StateComponent)) }
            });
        }
        protected override void OnUpdate()
        {
            var systemDeps = Dependency;
            systemDeps = new MoveToMeleeRange() {
                MoveChunk = GetComponentTypeHandle<Movement>(false),
                TargetChunk = GetComponentTypeHandle<AttackTarget>(true)
            }.Schedule(meleeTagAdded, systemDeps);
            systemDeps = new InAttackRange().Schedule( systemDeps);

            Dependency = systemDeps;
        }

        [BurstCompile]
        partial struct MoveToMeleeRange : IJobChunk {
            public ComponentTypeHandle<Movement> MoveChunk;
           [ReadOnly] public ComponentTypeHandle<AttackTarget> TargetChunk;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Movement> moves = chunk.GetNativeArray(ref MoveChunk);
                NativeArray<AttackTarget> targets = chunk.GetNativeArray(ref TargetChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    moves[i].SetLocation(targets[i].AttackTargetLocation);
                }
            }
        }
        partial struct MoveToFiringRange : IJobChunk
        {
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                throw new System.NotImplementedException();
            }
        }
        [BurstCompile]
        partial struct InAttackRange: IJobEntity {
            public float DeltaTime;
            void Execute(ref Movement move, ref MeleeAttackSubState melee,ref meleeAttackTag tag, ref AttackTarget target, in LocalTransform transform) {
                if (move.Completed) {
                    if (tag.AttackDelay > 0.0F)
                        tag.AttackDelay -= DeltaTime;
                    else {
                        Debug.Log("Trigger Attack animation");
                        tag.AttackDelay = 6.85f; //Todo Set this is be a random number later based off stats
                    }
                    if (Vector3.Distance(target.AttackTargetLocation, transform.Position) > melee.AttackRange) { 
                        move.SetLocation(target.AttackTargetLocation);
                    }
                }
            }
        }
    }
}
