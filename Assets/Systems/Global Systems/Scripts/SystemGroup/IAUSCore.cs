
using Unity.Entities;


namespace IAUS.Core
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class IAUS_UpdateSystem : ComponentSystemGroup { }

    [UpdateInGroup(typeof(IAUS_UpdateSystem))]
    [UpdateAfter(typeof(IAUS_UpdateScore))]
    public partial class IAUS_UpdateState: ComponentSystemGroup { }

    
    
    [UpdateBefore(typeof(IAUS_UpdateState))]
    [UpdateInGroup(typeof(IAUS_UpdateSystem))]
    public partial class IAUS_UpdateConsideration : ComponentSystemGroup { }
    [UpdateAfter(typeof(IAUS_UpdateConsideration))]
    [UpdateInGroup(typeof(IAUS_UpdateSystem))]
    public partial class IAUS_UpdateScore: ComponentSystemGroup { }
    [UpdateAfter(typeof(IAUS_UpdateSystem))]
   // [UpdateInGroup(typeof(PresentationSystemGroup))]

    public partial class IAUS_Reactions : ComponentSystemGroup { }


    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class IAUS_Initialization : ComponentSystemGroup { }
}