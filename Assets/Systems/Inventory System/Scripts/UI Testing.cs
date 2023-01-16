using Dreamers.InventorySystem;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class UITesting : MonoBehaviour
{
    public Button buttonPrefab;
    public Canvas Canvas;


    class baking : Baker<UITesting>
    {
        public override void Bake(UITesting authoring)
        {
            AddComponentObject(new UITestingC()
            {
                buttonPrefab = authoring.buttonPrefab,
                Canvas = authoring.Canvas
            });
        }
    }
}
public class UITestingC : IComponentData {
    public Button buttonPrefab;
    public Canvas Canvas;

}
public partial class UIInventorySystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(World.Unmanaged);
        var ui = new UITestingC();
        Entities.WithoutBurst().ForEach((UITestingC le) => {
            ui = le;
        }).Run();


        Entities.WithoutBurst().ForEach((CharacterInventory inventory, BaseCharacterComponent player, ref PlayerTag tag) => {
        
        
        }).Run();
    }
}