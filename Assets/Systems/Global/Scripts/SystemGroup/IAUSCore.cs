
using Unity.Entities;


namespace IAUS.Core
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class IAUS_UpdateSystem : ComponentSystemGroup { }

    [UpdateInGroup(typeof(IAUS_UpdateSystem))]
    [UpdateAfter(typeof(IAUS_UpdateScore))]
    public class IAUS_UpdateState: ComponentSystemGroup { }

    
    
    [UpdateBefore(typeof(IAUS_UpdateState))]
    [UpdateInGroup(typeof(IAUS_UpdateSystem))]
    public class IAUS_UpdateConsideration : ComponentSystemGroup { }
    [UpdateAfter(typeof(IAUS_UpdateConsideration))]
    [UpdateInGroup(typeof(IAUS_UpdateSystem))]
    public class IAUS_UpdateScore: ComponentSystemGroup { }
    [UpdateAfter(typeof(IAUS_UpdateSystem))]
   // [UpdateInGroup(typeof(PresentationSystemGroup))]

    public class IAUS_Reactions : ComponentSystemGroup { }


    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class IAUS_Initialization : ComponentSystemGroup { }
}