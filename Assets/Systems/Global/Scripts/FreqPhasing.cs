using Unity.Entities;


namespace Dreamers.Global
{
    [GenerateAuthoringComponent]
    public struct FreqPhasing : IComponentData
    {
        public int Phasing;
    }
}