using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using System.Threading.Tasks;
using System;
using Unity.Entities;
namespace Stats.UI
{
    public class StatsUI : MonoBehaviour
    {
        [SerializeField] BaseCharacter character;

        PlayerStatComponent test;
               [SerializeField] List<Bar> healthBars;
        [SerializeField] List<Bar> manaBars;


        // Start is called before the first frame update
        void Start()
        {
            Setup();
        }

        // Update is called once per frame
        void Update()
        {
            
        }
        public void UpdateHealthBar(int CurHP) {

            foreach (Bar bar in healthBars)
            {
                bar.updateBar(CurHP);
            }
        }
        public void UpdateManaBar(int CurMP) {
            foreach (Bar bar in manaBars)
            {
                bar.updateBar(CurMP);
            }
        }

        async void Setup() { 
            await Task.Delay(TimeSpan.FromSeconds(2));
            character = FindObjectOfType<PlayerCharacter>();
            foreach (Bar bar in healthBars)
            {
                bar.init(character.MaxHealth);
            }
            foreach (Bar bar in manaBars)
            {
                bar.init(character.MaxMana);
            }
            
        }
    }
    [System.Serializable]
    public class Bar {

        public bool depleted => BarSprite.fillAmount <= 0.0f;
        public int Order;
        public float fillAmount;
        [SerializeField] Image BarSprite;
        public float2 Bounds; //X equals lower bounds y = upper
         int maxValue;
       [SerializeField] int2 valueRange => (int2)(Bounds * maxValue);
        int barMaxValue => valueRange.y - valueRange.x;

        public void init(int maxHealth) {
            this.maxValue = maxHealth;
            BarSprite.type = Image.Type.Filled;
            BarSprite.fillMethod = Image.FillMethod.Horizontal;
            updateBar(maxValue);
        }
       float SetFillAmount(int curValue) {
            int value = Mathf.Clamp( curValue - valueRange.x, 0,  90000);
           return fillAmount = Mathf.Clamp01( value / (float)barMaxValue);
        }

        public void updateBar(int value) {
            BarSprite.fillAmount = SetFillAmount(value);
        }

    }
}
