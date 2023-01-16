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

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        class UIStatBaker : Baker<UIStat>
        {
            public override void Bake(UIStat authoring)
            {
                PlayerCharacterUI uI = new PlayerCharacterUI()
                {
                    HealthBar = authoring.HealthBar,
                    ManaBar = authoring.ManaBar,
                };
                AddComponentObject(uI); 
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