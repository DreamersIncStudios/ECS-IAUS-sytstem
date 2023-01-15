using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

[GenerateAuthoringComponent]
public struct Player_Control : IComponentData {
    public ControllerScheme InputSet => MotionSystem.Controls.ControlMaster.Instance.controller;
    public bool Jump => Input.GetKeyUp(InputSet.Jump);
    public bool DisplayCombos => Input.GetKeyUp(KeyCode.JoystickButton7);
    public bool Block => Input.GetKeyDown(InputSet.Block);
    public bool LightAttack => Input.GetKeyUp(InputSet.LightAttack);
    public bool HeavyAttack => Input.GetKeyUp(InputSet.HeavyAttack);
    public bool ChargedLightAttack => Input.GetKey(InputSet.LightAttack); 
    public bool ChargedHeavyAttack => Input.GetKey(InputSet.HeavyAttack); 
    public bool Projectile => Input.GetKeyUp(InputSet.Projectile);
    public bool ChargedProjectile => Input.GetKey(InputSet.Projectile);

    public bool Charged => ChargedTime > 2.5f;
    [HideInInspector]public float ChargedTime;
    public float ChargeTime;

    public bool InSafeZone;

}

public class ChargeSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Player_Control PC) => {
            if (PC.ChargedHeavyAttack || PC.ChargedLightAttack || PC.ChargedProjectile)
                PC.ChargedTime += Time.DeltaTime;
        });
    }
}
