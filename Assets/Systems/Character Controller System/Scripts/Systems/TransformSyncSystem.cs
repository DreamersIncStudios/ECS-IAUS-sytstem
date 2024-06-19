using Unity.Entities;
using Unity.Transforms;

namespace MotionSystem
{
    public partial class TransformSyncSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((TransformGO go, ref LocalTransform local) =>
            {
                go.transform.position = local.Position;
                 go.transform.rotation = local.Rotation;
            }).Run();
        }
    }
}
