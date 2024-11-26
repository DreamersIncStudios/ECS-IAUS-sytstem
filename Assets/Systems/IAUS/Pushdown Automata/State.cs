namespace DreamersIncStudio.PushDown
{
    public abstract class State
    {
        protected readonly PushdownAutomata pda;

        protected State(PushdownAutomata pda)
        {
            this.pda = pda;
        }
        public abstract void OnEnter();
        public abstract void OnExit();
        public abstract void OnUpdate();
    }
}