using Unity.Entities;


namespace IAUS.ECS2
{
    public struct DetectionConsideration : IComponentData
    {
        public float VisibilityRatio;
        public float RangeRatio;

    }
}