using Unity.Entities;
using Unity.Mathematics;
namespace IAUS.ECS2.BackGround.Raycasting
{
    [GenerateAuthoringComponent]
    public struct HumanRayCastPoints : IComponentData
    {
        public float3 Head { get; set; }
        public float3 Chest { get; set; }
        public float3 Left_Arm { get; set; }
        public float3 Right_Arm { get; set; }
        public float3 Left_Leg { get; set; }
        public float3 Right_Leg { get; set; }
    }

   

}

