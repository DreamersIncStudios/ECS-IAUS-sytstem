using DreamersInc;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Stats.Entities
{

    public class UIStat : MonoBehaviour
    {
        public Image HealthBar, ManaBar;

        class UIStatBaker : Baker<UIStat>
        {
            public override void Bake(UIStat authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                PlayerCharacterUI uI = new PlayerCharacterUI()
                {
                    HealthBar = authoring.HealthBar,
                    ManaBar = authoring.ManaBar,
                };
                AddComponentObject(entity,uI); 
            }
        }
    }

    public class PlayerCharacterUI : IComponentData {
        public Image HealthBar, ManaBar;

    }
    public partial class PlayerUISystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (SystemAPI.TryGetSingleton<Player_Control>(out _))
            {
                BaseCharacterComponent player = new BaseCharacterComponent();
                Entities.WithoutBurst().ForEach((BaseCharacterComponent Player, in PlayerTag tag) =>
                {
                    player = Player;
                }).Run();


                Entities.WithoutBurst().ForEach((PlayerCharacterUI UI) =>
                {
                    UI.HealthBar.fillAmount = player.HealthRatio;
                    UI.ManaBar.fillAmount = player.ManaRatio;


                }).Run();
            }
        }
    }
}