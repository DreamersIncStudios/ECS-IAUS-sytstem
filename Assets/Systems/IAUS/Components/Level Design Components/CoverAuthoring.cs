using Unity.Entities;
using UnityEngine;

namespace IAUS.LevelDesign
{
    public class CoverAuthoring : MonoBehaviour
    {
        public class CoverBaker : Baker<CoverAuthoring>
        {
            public override void Bake(CoverAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Cover>(entity);
            }
        }
    }
}