using Unity.Entities;
using UnityEngine;

namespace DreamersIncStudio.GAIACollective
{
    public class GaiaController : MonoBehaviour
    {
        [SerializeField] GaiaConfiguration configuration;
        [SerializeField] float startTimeOfDay;
        private class GaiaControllerBaker : Baker<GaiaController>
        {
            public override void Bake(GaiaController authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<RunningTag>(entity);
                AddComponent(entity,new GaiaLightSettings(authoring.configuration));
                AddComponent(entity, new GaiaTime(authoring.startTimeOfDay,authoring.configuration));
            }
        }
    }
}