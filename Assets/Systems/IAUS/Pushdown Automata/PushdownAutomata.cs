using System.Collections.Generic;
using UnityEngine;

namespace DreamersIncStudio.PushDown
{
    public class PushdownAutomata
    {
        readonly Stack<State> stateStack = new ();
        
        public State CurrentState => stateStack.Count>0 ? stateStack.Peek() : null;

        public Context Context;
        public PushdownAutomata(Context context)
        {
            this.Context = context;
        }
        
        public void PushState(State state)
        {
            stateStack.Push(state);
            state.OnEnter();
        }
        public void PopState()
        {
            if(stateStack.Count == 0) return;
            CurrentState.OnExit();
            stateStack.Pop();
        }
        public void Update() => CurrentState?.OnUpdate();
        
        // add new context  
        public void SetContext(Context context) => this.Context = context;
    }

  
}