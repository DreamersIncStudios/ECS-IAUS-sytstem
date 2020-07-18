using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
namespace SpawnerSystem
{
    [GenerateAuthoringComponent]
    public struct NpcTag : IComponentData
    {
        public int stupid { get { return 0; } } 
    }


}