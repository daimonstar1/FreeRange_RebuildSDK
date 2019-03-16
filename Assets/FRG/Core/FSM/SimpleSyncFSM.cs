namespace FRG.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A simplest possible FSM implementation that just acts as a helper to 
    /// keep the current state and call transition methods you provide yourself.
    /// You call <see cref="ChangeState(TState)"/> yourself.
    /// </summary>
    /// <typeparam name="TState">Pass an Enum for state Id.</typeparam>
    public class SimpleSyncFSM<TState>
        where TState : struct, IComparable, IConvertible, IFormattable
    {
        struct StateActions
        {
            public Action OnStateEnter;

            public Action OnStateExit;
        }

        private TState _currentState;

        public TState CurrentState
        {
            get { return _currentState; }
        }

        private TState _lastState;

        /// <summary>
        /// State FSM had before it got into CurrentState. Could be the same as CurrentState if FSM just initialized and didn't do any state changes yet.
        /// </summary>
        public TState LastState
        {
            get { return _lastState; }
        }

        private Dictionary<TState, StateActions> stateActions = new Dictionary<TState, StateActions>(EnumEqualityComparer<TState>.Default);

        /// <summary>
        /// Call to transition FSM to a new state. If specifixed in FSM, exit Action is called for old state, and then enter Action is called for new state.
        /// </summary>
        /// <returns>True if state changed, false if new state is the same as old one.</returns>
        public bool ChangeState(TState newState)
        {
            if (EnumEqualityComparer<TState>.Default.Equals(_currentState, newState))
                return false;

            _lastState = _currentState;
            StateActions actions = GetActions(newState, _lastState);
            if (actions.OnStateExit != null)
                actions.OnStateExit();
            _currentState = newState;
            if (actions.OnStateEnter != null)
                actions.OnStateEnter();

            return true;
        }

        public void Initialize(TState initialState)
        {
            _currentState = initialState;
            _lastState = initialState;
        }

        public void SetStateActions(TState state, Action onStateEnter, Action onStateExit)
        {
            stateActions[state] = new StateActions() { OnStateEnter = onStateEnter, OnStateExit = onStateExit };
        }

        /// <summary>
        /// Returns exit action for the old state and enter action for the new state.
        /// </summary>
        private StateActions GetActions(TState newState, TState lastState)
        {
            StateActions actions;
            Action exitAction = null;
            if (stateActions.TryGetValue(lastState, out actions))
            {
                exitAction = actions.OnStateExit;
            }
            Action enterAction = null;
            if (stateActions.TryGetValue(newState, out actions))
            {
                enterAction = actions.OnStateEnter;
            }

            return new StateActions() { OnStateEnter = enterAction, OnStateExit = exitAction };
        }
    }
}
