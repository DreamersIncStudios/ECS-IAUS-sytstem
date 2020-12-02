using Unity.Entities;

namespace IAUS.ECS2.Consideration
{
    public interface IBaseConsiderations : IComponentData
    {
        float Ratio { get; set; }
    }
}