using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System.Threading.Tasks;
using System;

namespace DreamersInc.ComboSystem.NPC
{
    [RequireComponent(typeof(Animator))]
    public class NPCComboComponentAuthoring : MonoBehaviour
    {
        public ComboSO Combo;


        Entity entity;
        public void SetupDataEntity(Entity entity)
        {
            EntityManager dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            dstManager.SetComponentData(entity, new NPCComboComponent()
            {
                Combo = Instantiate(Combo)
            });
        }


        public async void Setup()
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            EntityManager dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            ComboSO temp = Instantiate(Combo);
            temp.UpdateTotalProbability();
            var data = new NPCComboComponent() { Combo = temp };
            dstManager.AddComponentData(entity, data);

        }



    }
    public class NPCComboComponent : IComponentData
    {
        public ComboSO Combo;
    }
}