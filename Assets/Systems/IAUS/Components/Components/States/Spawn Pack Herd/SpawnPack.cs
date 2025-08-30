namespace IAUS.ECS.Component
{
    public struct SpawnPack : IBaseStateScorer
    {
        public void SetIndex(int index)
        {
            Index = index;
        }

        public int Index { get; private set; }
        public AIStates Name => AIStates.SpawnPackHerd;
        
        public float TotalScore { get => totalScore;
            set { totalScore = value; } }
        public ActionStatus Status { get { return status; } set { status = value; } }
        public float CoolDownTime { get { return coolDownTime; } }
        public bool InCooldown => Status == ActionStatus.CoolDown;
        public float ResetTime { get { return resetTime; } set { resetTime = value; } }
        public float mod { get { return 1.0f - (1.0f / 4.0f); } }

        float coolDownTime;
        float resetTime { get; set; }
        float totalScore { get; set; }
        ActionStatus status;
    }
}