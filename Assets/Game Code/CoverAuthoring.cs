using Global.Component;
using Unity.Entities;
using UnityEngine;

public class CoverAuthoring : MonoBehaviour
{
    public AITarget target;
    class baker:Baker<CoverAuthoring>
    {
        public override void Bake(CoverAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.WorldSpace);
            AddComponent(entity,  authoring.target);
        }
    }
}
