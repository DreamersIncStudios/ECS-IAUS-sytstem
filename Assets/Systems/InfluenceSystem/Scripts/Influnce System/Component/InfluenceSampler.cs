using Unity.Mathematics;
using Unity.Entities;

namespace InfluenceSystem.Component
{
    [GenerateAuthoringComponent]
    public struct InfluenceSampler : IComponentData
    {
        public float value;
    }
}