using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS2;
namespace IAUS.Core
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class IAUS_UpdateSystem : ComponentSystemGroup { }

    [UpdateInGroup(typeof(IAUS_UpdateSystem))]
    public class IAUS_UpdateState: ComponentSystemGroup { }

    
    
    [UpdateBefore(typeof(IAUS_UpdateState))]
    [UpdateInGroup(typeof(IAUS_UpdateSystem))]
    public class IAUS_UpdateConsideration : ComponentSystemGroup { }
    [UpdateAfter(typeof(IAUS_UpdateConsideration))]
    [UpdateInGroup(typeof(IAUS_UpdateSystem))]
    public class IAUS_UpdateScore: ComponentSystemGroup { }



    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class IAUS_Initialization : ComponentSystemGroup { }
}