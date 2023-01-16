using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DreamersInc
{
    [CreateAssetMenu(fileName = "InputData", menuName = "GameParts/InputField", order = 100)]

    // consider preset variations
    public class ControllerScheme : ScriptableObject, ButtonConfigs
    {
        [SerializeField] KeyCode _jump;
        [SerializeField] KeyCode _lightAttack;
        [SerializeField] KeyCode _heavyAttack;

        [SerializeField] KeyCode _block;
        [SerializeField] KeyCode _cadMenu;
        [SerializeField] KeyCode _projectile;

        public KeyCode Jump { get { return _jump; } set { _jump = value; } }
        public KeyCode LightAttack { get { return _lightAttack; } set { _lightAttack = value; } }
        public KeyCode HeavyAttack { get { return _heavyAttack; } set { _heavyAttack = value; } }

        public KeyCode Block { get { return _block; } set { _block = value; } }
        public KeyCode ActivateCADMenu { get { return _cadMenu; } set { _cadMenu = value; } }
        public KeyCode Projectile { get { return _projectile; } set { _projectile = value; } }

    }
    public struct ControllerInfo : IComponentData {

        public KeyCode Jump { get; private set;  }
        public KeyCode LightAttack { get; private set; }
        public KeyCode HeavyAttack { get; private set; }

        public KeyCode Block { get; private set; }
        public KeyCode ActivateCADMenu { get; private set; }
        public KeyCode Projectile { get; private set; }
        public bool Jumpb => Input.GetKeyUp(Jump);
        public bool DisplayMenu => Input.GetKeyUp(KeyCode.JoystickButton7);
        public bool Blockb => Input.GetKeyDown(Block);
        public bool LightAttackb => Input.GetKeyUp(LightAttack);
        public bool HeavyAttackb => Input.GetKeyUp(HeavyAttack);
        public bool ChargedLightAttackb => Input.GetKey(LightAttack);
        public bool ChargedHeavyAttackb => Input.GetKey(HeavyAttack);
        public bool Projectileb => Input.GetKeyUp(Projectile);
        public bool ChargedProjectileb => Input.GetKey(Projectile);
        public bool Casting;

        public bool Charged => ChargedTime > 2.5f;
        [HideInInspector] public float ChargedTime;
        public float ChargeTime;

        public bool InSafeZone;

        public void setup(ControllerScheme controller) {
            ActivateCADMenu = controller.ActivateCADMenu;
            LightAttack = controller.LightAttack;
            Block = controller.Block;
            HeavyAttack = controller.HeavyAttack;
            Jump = controller.Jump;
            Projectile = controller.Projectile;
        }
    }


    public enum PlatformOptions
    {
        PC, XBOX, PS4, Switch
    }
    public interface ButtonConfigs
    {
        KeyCode Jump { get; set; }
        KeyCode LightAttack { get; set; }
        KeyCode HeavyAttack { get; set; }
        KeyCode Block { get; set; }
        KeyCode ActivateCADMenu { get; set; }
        public KeyCode Projectile { get; set; }

    }
}