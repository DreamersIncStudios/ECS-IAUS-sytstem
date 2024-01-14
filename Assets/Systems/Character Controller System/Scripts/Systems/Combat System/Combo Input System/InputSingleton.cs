using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DreamersInc.InputSystems
{

    public class InputSingleton : IComponentData
    {
        public PlayerControls ControllerInput;
    }
}
