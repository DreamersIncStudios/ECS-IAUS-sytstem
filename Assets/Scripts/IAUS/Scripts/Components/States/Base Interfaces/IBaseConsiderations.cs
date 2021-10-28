using Unity.Entities;

namespace IAUS.ECS.Consideration
{
    public interface IBaseConsiderations : IComponentData
    {
        float Ratio { get; set; }
    }
}