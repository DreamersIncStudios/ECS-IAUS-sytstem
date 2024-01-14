using Unity.Entities;

namespace IAUS
{
    public struct IAUSBrain : IComponentData
    {
       
    }

    public struct SetupIAUSBrainTag : IComponentData
    {
    }
    
    public enum ActionStatus   
    {
        Idle, Success, Running, Interrupted, CoolDown, Disabled, Failure
    }
}
