using DreamersInc.DamageSystem;
using DreamersInc.DamageSystem.Interfaces;
using Stats.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static PrimeTween.Tween;

namespace DreamersInc.CombatSystem.UI
{
    public class DamagePopUpManager : MonoBehaviour
    {
        Transform mainCameraTransform;
        [SerializeField] GameObject damagePrefab;
        [SerializeField] Transform worldCanvasTransform;
        [SerializeField] float3 offset;
        private void Start()
        {
            mainCameraTransform = Camera.main.transform;
        }
        public void OnEnable()
        {
            DamageCheckSystem.DamagePushed += DisplayDamnageUI;

        }

        private void DisplayDamnageUI(object sender, OnDamageDealtEventArgs eventArgs)
        {
            var dirToCamera = (Vector3)eventArgs.position - mainCameraTransform.position;
            var rotToCamera = Quaternion.LookRotation(dirToCamera, Vector3.up);
            var damageUI = Instantiate(damagePrefab, worldCanvasTransform);
            damageUI.transform.position = eventArgs.position + offset;
            damageUI.transform.rotation = rotToCamera;

            var tmp = damageUI.GetComponent<TextMeshProUGUI>();
            tmp.text = eventArgs.damage.ToString();
            ShakeLocalPosition(transform, new Vector3(2.5f, 2.5f, 2.5f), .75f);
            Alpha(tmp, 0, 3.25f);
     //       tmp.DOFade(0, 3);
       //     damageUI.transform.DOShakePosition(2.5f, .25f);
            Destroy(damageUI, 3.5f);
            //Todo add color changes based on damage regular, heal, critical hit

        }

        private void OnDisable()
        {
            DamageCheckSystem.DamagePushed -= DisplayDamnageUI;
        }


    }
    [UpdateBefore(typeof(AdjustVitalSystems))]
    public partial class DamageCheckSystem : SystemBase
    {
        public static event EventHandler<OnDamageDealtEventArgs> DamagePushed;

        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((in AdjustHealth mod, in LocalToWorld transform) => {
                if (DamagePushed != null)
                    DamagePushed(this, new OnDamageDealtEventArgs() { damage = mod.Value, position = transform.Position });

            }).Run();
        }
    }
    public class OnDamageDealtEventArgs : EventArgs
    {
        public int damage;
        public float3 position;
    }
}