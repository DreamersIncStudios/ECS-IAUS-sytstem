using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace DreamersInc.ComboSystem.NPC
{
    [RequireComponent(typeof(Animator))]
    public class NPCComboComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public ComboSO Combo;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            ComboSO temp = Instantiate(Combo);
            temp.UpdateTotalProbability();
            var data = new NPCComboComponent() { animator = GetComponent<Animator>(), combo = temp};
            dstManager.AddComponentData(entity, data);
        }

    }
    public class NPCComboComponent : IComponentData {
        public ComboSO combo;
        public Animator animator;
    }
}