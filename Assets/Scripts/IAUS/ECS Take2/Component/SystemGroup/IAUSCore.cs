using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace IAUS.Core
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class IAUS_UpdateSystem : ComponentSystemGroup { }
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class IAUS_Initialization : ComponentSystemGroup { }
}