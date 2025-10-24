using Global.Component;
using Stats.Entities;
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
            AddComponent(entity, new AIStat()
            {
                CurHealth = 100,
                MaxHealth = 100,
                Speed = 2,
                CurMana = 100,
                MaxMana = 100,
                Level = 1,
            });
        }
    }
}
