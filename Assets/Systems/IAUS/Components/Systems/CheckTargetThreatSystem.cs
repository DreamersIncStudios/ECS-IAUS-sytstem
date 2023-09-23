using IAUS.ECS.Component;
using Unity.Entities;

namespace AISenses.VisionSystems
{
    [UpdateInGroup(typeof(VisionTargetingUpdateGroup))]
    [UpdateAfter(typeof(TargetingQuadrantSystem))]
    public partial class CheckTargetThreatSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(( DynamicBuffer<ScanPositionBuffer> buffer,ref IAUSBrain brain) =>
            {
                for (int i = 0; i <  buffer.Length; i++)
                {
                    var temp = buffer[i];
                    temp.target.CheckIsFriendly(brain.FactionID);
                    buffer[i] = temp;
                }
             
            }).WithoutBurst().Run();
        }
    }
}