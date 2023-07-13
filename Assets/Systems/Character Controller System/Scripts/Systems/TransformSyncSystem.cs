
using Unity.Entities;
using Unity.Transforms;

namespace MotionSystem
{
    public partial class TransformSyncSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((TransformGO go, ref LocalTransform local) => {
                local.Position = go.transform.position;
                local.Rotation = go.transform.rotation;
            }).Run();
        }
    }
}
