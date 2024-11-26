using Unity.Entities;

namespace DreamersIncStudio.PushDown
{
    public class PDAController : IComponentData
    {
        public PushdownAutomata pda;
     
    }
    public struct Context
    {
    }
}