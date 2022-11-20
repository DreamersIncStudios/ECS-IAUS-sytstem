using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using DreamersInc.ComboSystem.NPC;
using DreamersInc.ComboSystem;
using Components.MovementSystem;
using Stats;
using MotionSystem.Tower;
using PixelCrushers;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, IAUS.ECS.Systems.Reactive.AttackTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, IAUS.ECS.Systems.Reactive.AttackTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, IAUS.ECS.Systems.Reactive.AttackTagReactor>.ManageComponentRemovalJob))]


namespace IAUS.ECS.Systems.Reactive
{
    public struct AttackTagReactor : IComponentReactorTagsForAIStates<AttackActionTag, AttackTargetState>
    {
        public void ComponentAdded(Entity entity, ref AttackActionTag AttackTag, ref AttackTargetState AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
            AttackTag.StyleOfAttack = AIStateCompoment.HighScoreAttack.style;
            AttackTag.AttackLocation = AIStateCompoment.HighScoreAttack.AttackTarget.LastKnownPosition;
            AttackTag.moveSet= AttackTag.CanAttack = false;
            AttackTag.attackThis = AIStateCompoment.HighScoreAttack.AttackTarget.entity;
            AttackTag.SubState = AttackSubStates.Chase_MoveIntoAttackRange;
        }

        public void ComponentRemoved(Entity entity, ref AttackTargetState AIStateCompoment, in AttackActionTag oldComponent)
        {
            if ( AIStateCompoment.Status == ActionStatus.Success)
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = 5.0f; // TODO assign in editor 
            }
            else
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = 7.5f ;

            }
        }

        public void ComponentValueChanged(Entity entity, ref AttackActionTag newComponent, ref AttackTargetState AIStateCompoment, in AttackActionTag oldComponent)
        {
        }

        public class AttackReactiveSystem : AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>
        {
            protected override AttackTagReactor CreateComponentReactor()
            {
                return new AttackTagReactor();
            }
        }

    }
    
    [UpdateAfter(typeof(AttackTagReactor.AttackReactiveSystem))]
    public partial class MobileUnitsAttackSystem : SystemBase
    {
         EntityQuery MeleeAttack;
        EntityQuery PatrolReactor;
         EntityQuery RangeAttack;
         EntityQuery AttackTagRemoved;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            MeleeAttack = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTypeInfo)),
                    ComponentType.ReadWrite(typeof(AttackTargetState)),
                    ComponentType.ReadWrite(typeof(AttackActionTag)),
                    ComponentType.ReadWrite(typeof(Movement)),
                    ComponentType.ReadOnly(typeof(EnemyStats)),
                    ComponentType.ReadOnly(typeof(IAUSBrain))}
            });
            PatrolReactor = GetEntityQuery(new EntityQueryDesc()
            {
                All= new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTypeInfo)), ComponentType.ReadWrite(typeof(AttackActionTag)),
                ComponentType.ReadWrite(typeof(Patrol)),ComponentType.ReadWrite(typeof(Movement))},
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>.StateComponent)) }
            });
        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            systemDeps = new PatrolUpdate() {
                MoveChunk = GetComponentTypeHandle<Movement>(false),
                PatrolChunk = GetComponentTypeHandle<Patrol>(false)
            }.Schedule(PatrolReactor, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new MoveToTarget()
            {
                MoveChunk = GetComponentTypeHandle<Movement>(false),
                StateChunk = GetComponentTypeHandle<AttackTargetState>(true),
                TagChunk = GetComponentTypeHandle<AttackActionTag>(false),
                Position = GetComponentDataFromEntity<LocalToWorld>(true)
            }.Schedule(MeleeAttack, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new AttackTarget()
            {
                StateChunk = GetComponentTypeHandle<AttackTargetState>(true),
                TagChunk = GetComponentTypeHandle<AttackActionTag>(false),
                Position = GetComponentDataFromEntity<LocalToWorld>(true),
                DT= Time.DeltaTime
            }.Schedule(MeleeAttack, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            Dependency = systemDeps;
        }

        struct MoveToTarget : IJobChunk
        {
            public ComponentTypeHandle<AttackActionTag> TagChunk;
           [ReadOnly] public ComponentTypeHandle<AttackTargetState> StateChunk;
            public ComponentTypeHandle<Movement> MoveChunk;
          [ReadOnly]  [NativeDisableParallelForRestriction] public ComponentDataFromEntity<LocalToWorld> Position;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<AttackActionTag> tags = chunk.GetNativeArray(TagChunk);
                NativeArray<AttackTargetState> states = chunk.GetNativeArray(StateChunk);
                NativeArray<Movement> moves = chunk.GetNativeArray(MoveChunk);
                for (int i = 0; i < chunk.Count; i++) {
                    if (tags[i].SubState != AttackSubStates.Chase_MoveIntoAttackRange)
                        continue;

                    AttackActionTag tag = tags[i];
                    AttackTargetState state = states[i];
                    Movement movement = moves[i];
                    if (!tag.moveSet)
                    {
                        movement.SetLocation(tag.AttackLocation);
                        movement.CanMove= tag.moveSet = true;
                    }
                    if (state.HighScoreAttack.InRangeForAttack(movement.DistanceRemaining))
                    {
                        tag.CanAttack = true;
                        tag.SubState = AttackSubStates.Attack;
                    }
                    else 
                    {
                        float dist = Vector3.Distance(Position[tag.attackThis].Position, tag.AttackLocation);
                        if (dist > 5) { 
                            tag.moveSet = false;
                            tag.AttackLocation = Position[tag.attackThis].Position;
                        }
                    }

                    moves[i] = movement;
                    tags[i] = tag;
                }
            }
        }
        struct AttackTarget : IJobChunk
        {
            public ComponentTypeHandle<AttackActionTag> TagChunk;
            [ReadOnly] public ComponentTypeHandle<AttackTargetState> StateChunk;
            public float DT;
            [ReadOnly][NativeDisableParallelForRestriction]public ComponentDataFromEntity<LocalToWorld> Position;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<AttackActionTag> tags = chunk.GetNativeArray(TagChunk);
                NativeArray<AttackTargetState> states = chunk.GetNativeArray(StateChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    if (tags[i].SubState != AttackSubStates.Attack)
                        continue;

                    AttackActionTag tag = tags[i];
                    AttackTargetState state = states[i];
                    if (tag.InCoolDown)
                    {
                        tag.CoolDownTime -= DT;
                    }
                    else
                    {
                        Debug.Log("Trigger Attack ");
                        tag.CoolDownTime = 15;
                        float dist = Vector3.Distance(Position[tag.attackThis].Position, tag.AttackLocation);
                        if (dist > 5)
                        {
                            tag.moveSet = false;
                            tag.AttackLocation = Position[tag.attackThis].Position;
                            tag.SubState = AttackSubStates.Chase_MoveIntoAttackRange;
                        }
                    }

                    tags[i] = tag;
                }
            }
        }

        struct PatrolUpdate : IJobChunk
        {
            public ComponentTypeHandle<Movement> MoveChunk;
            public ComponentTypeHandle<Patrol> PatrolChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Movement> moves = chunk.GetNativeArray(MoveChunk);
                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Debug.Log("added");
                    Movement movement = moves[i];
                    Patrol patrol = patrols[i];

                    movement.CanMove = false;
                    patrol.AttackTarget = true;

                    patrols[i] = patrol;
                    moves[i] = movement;
                }
            }
        }

    }

}