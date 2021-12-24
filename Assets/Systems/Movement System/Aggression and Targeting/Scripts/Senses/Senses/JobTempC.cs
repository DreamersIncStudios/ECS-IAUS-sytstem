using Unity.Entities;

namespace AISenses
{
    [GenerateAuthoringComponent]
    public struct JobTempC : IComponentData
    {
        public Jobs job;
    }
}