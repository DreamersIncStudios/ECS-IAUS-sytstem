using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
namespace DreamersInc.InflunceMapSystem
{

    public class InfluenceAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public InfluenceComponent data;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, data);
            var Perception = new Perceptibility() { movement = MovementStates.Stadning_Still, noiseState = NoiseState.Normal, visibilityStates = VisibilityStates.Visible };
            dstManager.AddComponentData(entity, Perception);

        }

        public void SetInfluence(int Threat, int Protect, int FactionID) {
            data.factionID = FactionID;
            data.Threat = Threat;
            data.Protection = Protect;
        }

    }
}
