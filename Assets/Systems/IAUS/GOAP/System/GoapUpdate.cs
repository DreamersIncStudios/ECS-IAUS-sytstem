using Unity.Entities;

namespace IAUS.Core.GOAP.System
{
    public partial class GoapUpdate : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((AttackGoapAgent agent, AttackGOAPAspect aspect) =>
            {
                agent.TargetPosition = aspect.TargetPosition;
                agent.MyPosition = aspect.MyPosition;
                
            }).Run();
        }
    }
}