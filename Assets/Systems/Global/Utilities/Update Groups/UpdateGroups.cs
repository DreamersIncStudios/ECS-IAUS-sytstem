using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public partial class IAUSUpdateBrainGroup : ComponentSystemGroup
{
    public IAUSUpdateBrainGroup()
    {
        RateManager = new RateUtils.VariableRateManager(240, true);

    }

}
public partial class IAUSAttackGroup : ComponentSystemGroup
{
    public IAUSAttackGroup()
    {
        RateManager = new RateUtils.VariableRateManager(250, true);

    }

}
public partial class IAUSUpdateStateGroup : ComponentSystemGroup
{
 
}
[UpdateBefore(typeof(IAUSUpdateBrainGroup))]
public partial class VisionTargetingUpdateGroup : ComponentSystemGroup
{
    public VisionTargetingUpdateGroup()
    {
        RateManager = new RateUtils.VariableRateManager(120, true);
    }

}