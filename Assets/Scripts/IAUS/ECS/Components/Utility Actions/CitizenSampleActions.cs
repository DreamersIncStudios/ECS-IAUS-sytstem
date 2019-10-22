

namespace IAUS.ECS.Component {

    public struct GetMoney : BaseUtilityAction
    {
        public float Score { get; set; }
        public States state { get; set; }
        public float Cooldown { get; set; }
        public float Timer { get; set; }

        public bool InCooldown()
        {
                return Timer > 0.0f;
        }
    }
    public struct GoBuyStuff: BaseUtilityAction
    {
        public float Score { get; set; }
        public States state { get; set; }
        public float Cooldown { get; set; }
        public float Timer { get; set; }

        public bool InCooldown()
        {
            return Timer > 0.0f;
        }
    }
    public struct TakeStuffHome : BaseUtilityAction
    {
        public float Score { get; set; }
        public States state { get; set; }
        public float Cooldown { get; set; }
        public float Timer { get; set; }

        public bool InCooldown()
        {
            return Timer > 0.0f;
        }
    }

    public struct EvadeRobberFindCop : BaseUtilityAction
    {
        public float Score { get; set; }
        public States state { get; set; }
        public float Cooldown { get; set; }
        public float Timer { get; set; }

        public bool InCooldown()
        {
            return Timer > 0.0f;
        }
    }
}
