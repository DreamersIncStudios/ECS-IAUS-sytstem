using IAUS.ECS.Component;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace IAUS.Components.Systems
{
    public partial struct FindInteractables : IJobEntity
    {
        public NativeArray<LocalTransform> InteractablesPosition;
        void Execute(ref TerrorizeAreaState data, in LocalTransform transform)
        {
        }
    }
}