using Unity.Entities;

namespace DreamersIncStudio.GAIACollective
{
    internal partial class GaiaUpdateGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<RunningTag>();
            RequireForUpdate<GaiaTime>();
            RequireForUpdate<WorldManager>();
        }
        public GaiaUpdateGroup()
        {
            RateManager = new RateUtils.VariableRateManager(80, true);
        }
    }
}