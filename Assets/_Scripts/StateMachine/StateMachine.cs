using System.Collections.Generic;
using System;


    public class StateMachine {

        private State _currentState;
        private State _queuedState;
        private Dictionary<Type, State> _states = new Dictionary<Type, State>();

        public StateMachine(List<State> states) {

            foreach (State state in states) {
                State instance = state;
                instance.stateMachine = this;
                _states.Add(instance.GetType(), instance);
                _currentState ??= instance;
            }

            _queuedState = _currentState;
            _currentState?.Enter();
        }

        public void Run() {
            if (_currentState != _queuedState) {
                _currentState.Exit();
                _currentState = _queuedState;
                _currentState.Enter();
            }

            _currentState.Run();
        }

        public void TransitionTo<T>() where T : State {
            _queuedState = _states[typeof(T)];
        }
    }
