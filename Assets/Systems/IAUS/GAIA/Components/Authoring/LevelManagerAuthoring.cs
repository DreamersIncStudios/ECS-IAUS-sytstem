
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DreamersIncStudio.GAIACollective.Authoring
{
    public class LevelManagerAuthoring : MonoBehaviour
    {
        public SpawnScenario SpawnScenario;
        public int2 Bounds;
        public class baker: Baker<LevelManagerAuthoring>
        {
            public override void Bake(LevelManagerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.WorldSpace);
                AddComponent(entity, new GaiaLevelManager()
                {
                    SpawnScenario =  authoring.SpawnScenario,
                    Bounds = authoring.Bounds
                });
            }
        }
    }
}