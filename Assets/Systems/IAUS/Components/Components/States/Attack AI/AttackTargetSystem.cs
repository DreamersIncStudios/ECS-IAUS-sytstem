using DreamersInc.InflunceMapSystem;
using IAUS.ECS.Component;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;


namespace AISenses.VisionSystems.Combat
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [UpdateAfter(typeof(TargetingSystem))]
    public partial class AttackTargetSystem : SystemBase
    {

        protected override void OnUpdate()
        {


        }
          
    }
}