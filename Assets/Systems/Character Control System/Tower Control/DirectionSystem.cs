using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Utilities.ReactiveSystem;
using MotionSystem.Tower;
using Unity.Jobs;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<RotateTowerTag, TowerController, MotionSystem.Reactive.RotationReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<RotateTowerTag, TowerController, MotionSystem.Reactive.RotationReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<RotateTowerTag, TowerController, MotionSystem.Reactive.RotationReactor>.ManageComponentRemovalJob))]

namespace MotionSystem.Tower
{


    public struct RotateTowerTag : IComponentData
    {
        public float3 TargetPosition;
        public float AngleOfRot;
    }
}




namespace MotionSystem.Reactive
{
    public struct RotationReactor : IComponentReactorTagsForAIStates<RotateTowerTag, TowerController>
    {
        public void ComponentAdded(Entity entity, ref RotateTowerTag Tag, ref TowerController Tower)
        {
                Tower.dirToTarget = ((Vector3)Tower.TargetLocation - (Vector3)(Tower.Position)).normalized;
            Tag.AngleOfRot= Vector3.Angle(Tower.forward, Tower.dirToTarget);

        }

        public void ComponentRemoved(Entity entity, ref TowerController AIStateCompoment, in RotateTowerTag oldComponent)
        {
        
        }

        public void ComponentValueChanged(Entity entity, ref RotateTowerTag newComponent, ref TowerController AIStateCompoment, in RotateTowerTag oldComponent)
        {
         
        }
        public class RotationReactiveSystem : AIReactiveSystemBase<RotateTowerTag, TowerController, RotationReactor> {
            protected override RotationReactor CreateComponentReactor()
            {
                return new RotationReactor();
            }
        }
    }
    public partial class DirectionSystem : SystemBase
    {
        EntityQuery TagAdded;
        EntityQuery TagRemoved;
        EntityQuery TagValueChange;

        protected override void OnCreate()
        {
            base.OnCreate();
            TagAdded = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(StaticObjectControllerAuthoring)),ComponentType.ReadOnly(typeof(RotateTowerTag)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<RotateTowerTag, TowerController,RotationReactor>.StateComponent))}

            });
            TagRemoved = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(StaticObjectControllerAuthoring)), ComponentType.ReadOnly(typeof(RotateTowerTag)), ComponentType.ReadOnly(typeof(AIReactiveSystemBase<RotateTowerTag, TowerController, RotationReactor>.StateComponent)) },

            });
            TagValueChange = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(StaticObjectControllerAuthoring)), ComponentType.ReadOnly(typeof(AIReactiveSystemBase<RotateTowerTag, TowerController, RotationReactor>.StateComponent)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(RotateTowerTag)) }

            });
        }

        protected override void OnUpdate()
        {
            Entities.WithoutBurst().WithAll<RotateTowerTag>().ForEach((Transform transform, ref RotateTowerTag Rot, ref TowerController Tower) => {

                transform.rotation = 
                Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, Rot.AngleOfRot, 0),Time.DeltaTime*Tower.RotateSpeed);
            
            }).Run();
        }
    }
}