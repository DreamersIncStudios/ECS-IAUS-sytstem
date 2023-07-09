using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Jobs.LowLevel;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using Components.MovementSystem;
using Unity.Burst.Intrinsics;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<WaitActionTag, Wait, IAUS.ECS.Systems.Reactive.WaitTagReactor>.StateComponent))]

[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<WaitActionTag, Wait, IAUS.ECS.Systems.Reactive.WaitTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<WaitActionTag, Wait, IAUS.ECS.Systems.Reactive.WaitTagReactor>.ManageComponentRemovalJob))]
[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<PatrolActionTag, Wait, IAUS.ECS.Systems.Reactive.PatrolWaitTagReactor>.StateComponent))]

[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<PatrolActionTag, Wait, IAUS.ECS.Systems.Reactive.PatrolWaitTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<PatrolActionTag, Wait, IAUS.ECS.Systems.Reactive.PatrolWaitTagReactor>.ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive
{
    public struct WaitTagReactor : IComponentReactorTagsForAIStates<WaitActionTag, Wait>
    {
        public void ComponentAdded(Entity entity, ref WaitActionTag newComponent, ref Wait AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref Wait AIStateCompoment, in WaitActionTag oldComponent)
        {
            if (AIStateCompoment.Complete)
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
            }
            else
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime * 2;

            }
            AIStateCompoment.Timer = 0.0f;
        }

        public void ComponentValueChanged(Entity entity, ref WaitActionTag newComponent, ref Wait AIStateCompoment, in WaitActionTag oldComponent)
        {
            Debug.Log("Change");
        }

        public partial class WaitReactiveSystem : AIReactiveSystemBase<WaitActionTag, Wait, WaitTagReactor>
        {
            protected override WaitTagReactor CreateComponentReactor()
            {
                return new WaitTagReactor();
            }

        }
    }

    public struct PatrolWaitTagReactor : IComponentReactorTagsForAIStates<PatrolActionTag, Wait>
    {
        public void ComponentAdded(Entity entity, ref PatrolActionTag newComponent, ref Wait AIStateCompoment)
        {
          
        }

        public void ComponentRemoved(Entity entity, ref Wait AIStateCompoment, in PatrolActionTag oldComponent)
        {
            AIStateCompoment.StartTime=AIStateCompoment.Timer = 15.5f; // TODO figure out what oldComponent.WaitTime = 0
        }

        public void ComponentValueChanged(Entity entity, ref PatrolActionTag newComponent, ref Wait AIStateCompoment, in PatrolActionTag oldComponent)
        {
        }
        public partial class WaitReactiveSystem2 : AIReactiveSystemBase<PatrolActionTag, Wait, PatrolWaitTagReactor>
        {
            protected override PatrolWaitTagReactor CreateComponentReactor()
            {
                return new PatrolWaitTagReactor();
            }

        }
    }

    public partial class WaitFinished : SystemBase
    {
        EntityQuery waiters;
        protected override void OnCreate()
        {
            base.OnCreate();
            waiters = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] {ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadWrite(typeof(LocalTransform)), ComponentType.ReadWrite(typeof(TravelWaypointBuffer)),
                    ComponentType.ReadWrite(typeof(AIReactiveSystemBase < WaitActionTag, Wait, WaitTagReactor >.StateComponent ))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(WaitActionTag)) }
            });
        }

        protected override void OnUpdate()
        {
            new Test().Schedule(waiters);

        }
    }
    [BurstCompile]
    partial struct Test : IJobEntity {

        public void Execute( ref Patrol patrol, in LocalTransform ToWorld, in DynamicBuffer<TravelWaypointBuffer> waypointBuffer) {
            if (patrol.WaypointIndex >= patrol.NumberOfWayPoints-1)
            {
                patrol.WaypointIndex = 0;
            }
            else
            {
                patrol.WaypointIndex++;
            }

            patrol.CurWaypoint = waypointBuffer[patrol.WaypointIndex].WayPoint;
            patrol.StartingDistance = Vector3.Distance(ToWorld.Position, patrol.CurWaypoint.Position);
        }
    
    }

   
    

}